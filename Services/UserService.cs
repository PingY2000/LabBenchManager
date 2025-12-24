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

        public UserService(IDbContextFactory<LabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

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

        // 🔥 修改：检查空字符串
        public async Task<string> GetDisplayNameOrUserNameAsync(string ntAccount)
        {
            var displayName = await GetUserDisplayNameAsync(ntAccount);
            // 🔥 空字符串也视为无效
            return !string.IsNullOrWhiteSpace(displayName) ? displayName : GetUserName(ntAccount);
        }

        // 🔥 修改：返回 null 如果是空字符串
        public async Task<string?> GetUserDisplayNameAsync(string ntAccount)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var normalizedNtAccount = ntAccount.ToLower();

            var displayName = await db.ApplicationUsers
                .Where(u => u.NtAccount.ToLower() == normalizedNtAccount)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync();

            // 🔥 空字符串视为 null
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

            // 🔥 空字符串视为 null
            return string.IsNullOrWhiteSpace(department) ? null : department;
        }

        // 🔥 修改：处理空字符串
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

            // 🔥 空字符串转为 null
            return (
                string.IsNullOrWhiteSpace(userInfo.DisplayName) ? null : userInfo.DisplayName,
                string.IsNullOrWhiteSpace(userInfo.Department) ? null : userInfo.Department
            );
        }

        // 🔥 修改：批量获取时处理空字符串
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
                u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : GetUserName(u.NtAccount), // 🔥 检查空字符串
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