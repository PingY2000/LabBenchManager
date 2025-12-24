// Services/UserService.cs
using DocumentFormat.OpenXml.InkML;
using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<LabDbContext> _dbFactory;
        private readonly ILogger<UserService> _logger; // 🆕 添加日志

        public UserService(IDbContextFactory<LabDbContext> dbFactory, ILogger<UserService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger; // 🆕
        }
        // <summary>
        /// 更新当前用户的个人信息（不包括角色）
        /// </summary>
        public async Task<bool> UpdateCurrentUserInfoAsync(
            string ntAccount,
            string? displayName,
            string? department,
            string? email)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            try
            {
                var user = await db.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == ntAccount.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("User '{NtAccount}' not found for info update.", ntAccount);
                    return false;
                }

                // 只更新个人信息字段，不更新角色
                user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
                user.Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
                user.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

                await db.SaveChangesAsync();

                _logger.LogInformation("Updated personal info for user '{NtAccount}'.", ntAccount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating personal info for user '{NtAccount}'.", ntAccount);
                return false;
            }
        }
        // ===== 🆕 新增方法：自动注册相关 =====

        /// <summary>
        /// 创建新用户（带并发处理）
        /// </summary>
        public async Task<ApplicationUser?> CreateUserAsync(ApplicationUser user)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            try
            {
                // 再次检查用户是否已存在（防止并发创建）
                var existing = await db.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == user.NtAccount.ToLower());

                if (existing != null)
                {
                    _logger.LogInformation("User '{NtAccount}' already exists (concurrent creation detected).", user.NtAccount);
                    return existing;
                }

                db.ApplicationUsers.Add(user);
                await db.SaveChangesAsync();

                _logger.LogInformation("User '{NtAccount}' created successfully with role '{Role}'.",
                    user.NtAccount, user.Role);

                return user;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true ||
                                                ex.InnerException?.Message.Contains("duplicate") == true)
            {
                // 处理唯一约束冲突（并发创建）
                _logger.LogWarning(ex, "Concurrent user creation detected for '{NtAccount}'. Fetching existing user.",
                    user.NtAccount);

                return await db.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == user.NtAccount.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user '{NtAccount}'.", user.NtAccount);
                return null;
            }
        }

        /// <summary>
        /// 更新用户角色
        /// </summary>
        public async Task<bool> UpdateUserRoleAsync(string ntAccount, string role)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            try
            {
                var user = await db.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == ntAccount.ToLower());

                if (user == null)
                {
                    _logger.LogWarning("User '{NtAccount}' not found for role update.", ntAccount);
                    return false;
                }

                user.Role = role;
                await db.SaveChangesAsync();

                _logger.LogInformation("Updated role to '{Role}' for user '{NtAccount}'.", role, ntAccount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user '{NtAccount}'.", ntAccount);
                return false;
            }
        }

        // ===== 原有方法保持不变 =====

        // 静态辅助方法 - 从 NT 账号提取用户名
        public static string GetUserName(string? ntAccount)
        {
            if (string.IsNullOrEmpty(ntAccount))
            {
                return "用户";
            }

            var nameParts = ntAccount.Split('\\');
            return nameParts.Length > 1 ? nameParts[1] : ntAccount;
        }

        public async Task<string> GetDisplayNameOrUserNameAsync(string ntAccount)
        {
            var displayName = await GetUserDisplayNameAsync(ntAccount);
            return !string.IsNullOrWhiteSpace(displayName) ? displayName : GetUserName(ntAccount);
        }

        public async Task<string?> GetUserDisplayNameAsync(string ntAccount)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedNtAccount = ntAccount.ToLower();

            var displayName = await db.ApplicationUsers
                .Where(u => u.NtAccount.ToLower() == normalizedNtAccount)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(displayName) ? null : displayName;
        }

        public async Task<string?> GetUserDepartmentAsync(string ntAccount)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedNtAccount = ntAccount.ToLower();

            var department = await db.ApplicationUsers
                .Where(u => u.NtAccount.ToLower() == normalizedNtAccount)
                .Select(u => u.Department)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(department) ? null : department;
        }

        public async Task<(string? DisplayName, string? Department)> GetUserInfoAsync(string ntAccount)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedNtAccount = ntAccount.ToLower();

            var userInfo = await db.ApplicationUsers
                .Where(u => u.NtAccount.ToLower() == normalizedNtAccount)
                .Select(u => new { u.DisplayName, u.Department })
                .FirstOrDefaultAsync();

            if (userInfo == null)
            {
                return (null, null);
            }

            return (
                string.IsNullOrWhiteSpace(userInfo.DisplayName) ? null : userInfo.DisplayName,
                string.IsNullOrWhiteSpace(userInfo.Department) ? null : userInfo.Department
            );
        }

        public async Task<Dictionary<string, string>> GetUserDisplayNamesAsync(IEnumerable<string> ntAccounts)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedAccounts = ntAccounts
                .Where(a => !string.IsNullOrEmpty(a))
                .Select(a => a.ToLower())
                .Distinct()
                .ToList();

            var dbUsers = await db.ApplicationUsers
                .Where(u => normalizedAccounts.Contains(u.NtAccount.ToLower()))
                .ToListAsync();

            return dbUsers.ToDictionary(
                u => u.NtAccount,
                u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : GetUserName(u.NtAccount),
                StringComparer.OrdinalIgnoreCase
            );
        }

        public async Task<ApplicationUser?> GetUserWithRoleAsync(string ntAccount)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedNtAccount = ntAccount.ToLower();

            return await db.ApplicationUsers
                .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == normalizedNtAccount);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            return await db.ApplicationUsers.OrderBy(u => u.NtAccount).ToListAsync();
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            db.ApplicationUsers.Add(user);
            await db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            db.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var user = await db.ApplicationUsers.FindAsync(id);
            if (user != null)
            {
                db.ApplicationUsers.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task<(int Added, int Updated)> UpsertUsersAsync(List<ApplicationUser> usersFromExcel)
        {
            if (usersFromExcel == null || !usersFromExcel.Any())
            {
                return (0, 0);
            }

            await using var db = await _dbFactory.CreateDbContextAsync();

            var addedCount = 0;
            var updatedCount = 0;

            var existingUsers = await db.ApplicationUsers
                .ToDictionaryAsync(u => u.NtAccount, StringComparer.OrdinalIgnoreCase);

            foreach (var user in usersFromExcel)
            {
                if (string.IsNullOrWhiteSpace(user.NtAccount)) continue;

                if (existingUsers.TryGetValue(user.NtAccount, out var existingUser))
                {
                    existingUser.DisplayName = user.DisplayName;
                    existingUser.Department = user.Department;
                    existingUser.Role = user.Role;
                    db.ApplicationUsers.Update(existingUser);
                    updatedCount++;
                }
                else
                {
                    db.ApplicationUsers.Add(user);
                    addedCount++;
                }
            }

            await db.SaveChangesAsync();
            return (addedCount, updatedCount);
        }

        public async Task<Dictionary<string, string>> GetDisplayNamesByAccountsAsync(List<string> ntAccounts)
        {
            if (ntAccounts == null || !ntAccounts.Any())
                return new Dictionary<string, string>();

            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedAccounts = ntAccounts
                .Where(a => !string.IsNullOrEmpty(a))
                .Select(a => a.ToLower())
                .Distinct()
                .ToList();

            var users = await db.ApplicationUsers
                .Where(u => normalizedAccounts.Contains(u.NtAccount.ToLower()))
                .Select(u => new { u.NtAccount, u.DisplayName })
                .ToListAsync();

            return users.ToDictionary(
                u => u.NtAccount.ToLower(),
                u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : GetUserName(u.NtAccount),
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}