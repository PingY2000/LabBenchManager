// Services/AssignmentService.cs
using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class AssignmentService
    {
        private readonly LabDbContext _db;

        public AssignmentService(LabDbContext db)
        {
            _db = db;
        }

        public async Task<List<Assignment>> GetAllAsync()
        {
            // 使用 .Include() 来预加载关联的 Bench 信息，这样就能在前端显示设备名称
            return await _db.Assignments
                            .Include(a => a.Bench)
                            .OrderByDescending(a => a.RequestTime)
                            .ToListAsync();
        }

        public async Task AddAsync(Assignment assignment)
        {
            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Assignment assignment)
        {
            _db.Entry(assignment).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<Assignment?> GetByIdAsync(int id)
        {
            return await _db.Assignments.FindAsync(id);
        }
    }
}