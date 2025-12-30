using LabBenchManager.Data;  
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LabBenchManager.Services
{
    public class TestPlanService
    {
        private readonly LabDbContext _db;

        public TestPlanService(LabDbContext db)
        {
            _db = db;
        }

        // 获取某个测试台的所有测试计划
        public async Task<List<TestPlan>> GetPlansByBenchIdAsync(int benchId)
        {
            // 新的排序逻辑：按计划的第一个日期排序
            var plans = await _db.TestPlans
                                 .Where(p => p.BenchId == benchId)
                                 .Include(p => p.Bench)
                                 .ToListAsync();

            // 在内存中排序，因为数据库无法直接解析 ScheduledDateList
            return plans.OrderBy(p => p.ScheduledDateList.FirstOrDefault()).ToList();
        }

        // 获取所有测试计划
        public async Task<List<TestPlan>> GetAllPlansAsync()
        {
            var plans = await _db.TestPlans
                                 .Include(p => p.Bench)
                                 .OrderBy(p => p.BenchId)
                                 .ToListAsync();

            // 在内存中排序
            return plans.OrderBy(p => p.BenchId)
                        .ThenBy(p => p.ScheduledDateList.FirstOrDefault())
                        .ToList();
        }

        // 根据ID获取单个计划
        public async Task<TestPlan?> GetByIdAsync(int id)
        {
            return await _db.TestPlans
                            .Include(p => p.Bench)
                            .FirstOrDefaultAsync(p => p.Id == id);
        }

        // 添加新测试计划
        public async Task AddAsync(TestPlan plan)
        {
            plan.CreatedAt = DateTime.Now;
            // 不再需要设置 SortOrder
            _db.TestPlans.Add(plan);
            await _db.SaveChangesAsync();
        }

        // 更新测试计划状态
        public async Task UpdateStatusAsync(int planId, TestPlanStatus newStatus)
        {
            var plan = await _db.TestPlans.FindAsync(planId);
            if (plan != null)
            {
                plan.Status = newStatus;
                plan.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
            }
        }

        // 更新测试计划
        public async Task UpdateAsync(TestPlan plan)
        {
            var existingPlan = await _db.TestPlans.FindAsync(plan.Id);
            if (existingPlan == null)
            {
                // 可以抛出异常或返回一个结果对象
                return;
            }

            // 手动更新字段，以避免EF Core跟踪问题
            existingPlan.ProjectName = plan.ProjectName;
            existingPlan.Description = plan.Description;
            existingPlan.Status = plan.Status;
            existingPlan.ScheduledDates = plan.ScheduledDates;
            existingPlan.AssignedTo = plan.AssignedTo;
            existingPlan.RequestedBy = plan.RequestedBy;
            existingPlan.SampleNumber = plan.SampleNumber;
            existingPlan.SampleQuantity = plan.SampleQuantity;
            existingPlan.Notes = plan.Notes;
            existingPlan.UpdatedAt = DateTime.Now;

            _db.TestPlans.Update(existingPlan);
            await _db.SaveChangesAsync();
        }

        // 删除测试计划
        public async Task DeleteAsync(int id)
        {
            var planToDelete = await _db.TestPlans.FindAsync(id);
            if (planToDelete != null)
            {
                if (planToDelete.AssignmentId.HasValue)
                {
                    var relatedAssignment = await _db.Assignments.FindAsync(planToDelete.AssignmentId.Value);
                    if (relatedAssignment != null)
                    {
                        relatedAssignment.TestPlanId = null;
                        if (relatedAssignment.Status == AssignmentStatus.已规划)
                        {
                            relatedAssignment.Status = AssignmentStatus.未开始;
                        }
                        _db.Assignments.Update(relatedAssignment);
                    }
                }

                _db.TestPlans.Remove(planToDelete);
                await _db.SaveChangesAsync(); // 原子操作，同时更新Assignment和删除TestPlan
            }
        }



    }
}