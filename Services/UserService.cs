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
    }
}