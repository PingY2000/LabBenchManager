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

        // ===============================================
        // == 新增的方法 ==
        // ===============================================
        /// <summary>
        /// 根据申请人的NT账号获取其提交的所有申请
        /// </summary>
        /// <param name="ntAccount">申请人的NT账号</param>
        /// <returns>一个包含该用户所有申请的列表</returns>
        public async Task<List<Assignment>> GetAssignmentsByNTAccountAsync(string? ntAccount)
        {
            if (string.IsNullOrEmpty(ntAccount))
            {
                return new List<Assignment>(); // 如果账号为空，返回空列表
            }

            return await _db.Assignments
                             .Where(a => a.ApplicantNTAccount == ntAccount)
                             .Include(a => a.Bench) // 同样可以预加载关联数据
                             .OrderByDescending(a => a.RequestTime)
                             .ToListAsync();
        }
        // ===============================================

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
            // 在获取单个申请时，也预加载关联数据，以便在详情页显示
            return await _db.Assignments
                            .Include(a => a.Bench)
                            .Include(a => a.TestPlan)
                            .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _db.Assignments.FindAsync(id);
            if (assignment != null)
            {
                // 如果有关联的 TestPlan，需要考虑如何处理
                // 方案1：不允许删除 (可以抛出异常或返回一个结果对象)
                if (assignment.TestPlanId.HasValue)
                {
                    throw new InvalidOperationException("无法删除已创建测试计划的申请。请先删除关联的测试计划。");
                }

                _db.Assignments.Remove(assignment);
                await _db.SaveChangesAsync();
            }
        }
    }
}