// Services/UserService.cs
using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class UserService
    {
        private readonly LabDbContext _db;

        public UserService(LabDbContext db)
        {
            _db = db;
        }

        // 方法名不变，但内部实现变了
        public async Task<ApplicationUser?> GetUserWithRoleAsync(string ntAccount)
        {
            // 将传入的参数和数据库中的字段都转换为小写（或大写）进行比较。
            // EF Core 能够将这个操作完美地翻译成 SQL 的 LOWER() 或 UPPER() 函数。
            var normalizedNtAccount = ntAccount.ToLower();

            return await _db.ApplicationUsers
                            .FirstOrDefaultAsync(u => u.NtAccount.ToLower() == normalizedNtAccount);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _db.ApplicationUsers.OrderBy(u => u.NtAccount).ToListAsync();
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            _db.ApplicationUsers.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _db.ApplicationUsers.FindAsync(id);
            if (user != null)
            {
                _db.ApplicationUsers.Remove(user);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(int Added, int Updated)> UpsertUsersAsync(List<ApplicationUser> usersFromExcel)
        {
            if (usersFromExcel == null || !usersFromExcel.Any())
            {
                return (0, 0);
            }

            var addedCount = 0;
            var updatedCount = 0;

            // 获取所有数据库中的NT账号，用于快速查找
            var existingUsers = await _db.ApplicationUsers.ToDictionaryAsync(u => u.NtAccount, StringComparer.OrdinalIgnoreCase);

            foreach (var user in usersFromExcel)
            {
                if (string.IsNullOrWhiteSpace(user.NtAccount)) continue; // 跳过无效数据

                if (existingUsers.TryGetValue(user.NtAccount, out var existingUser))
                {
                    // 更新现有用户
                    existingUser.DisplayName = user.DisplayName;
                    existingUser.Department = user.Department;
                    existingUser.Role = user.Role;
                    _db.ApplicationUsers.Update(existingUser);
                    updatedCount++;
                }
                else
                {
                    // 添加新用户
                    _db.ApplicationUsers.Add(user);
                    addedCount++;
                }
            }

            await _db.SaveChangesAsync();
            return (addedCount, updatedCount);
        }
    }
}