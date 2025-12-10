// Services/TestPlanService.cs
using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class TestPlanService
    {
        private readonly LabDbContext _db;

        public TestPlanService(LabDbContext db)
        {
            _db = db;
        }

        // 获取某个测试台的所有测试计划，按优先级和排序序号排序
        public async Task<List<TestPlan>> GetPlansByBenchIdAsync(int benchId)
        {
            return await _db.TestPlans
                            .Where(p => p.BenchId == benchId)
                            .OrderByDescending(p => p.Priority)  // 优先级高的在前
                            .ThenBy(p => p.SortOrder)            // 同优先级按排序号
                            .ThenBy(p => p.PlannedStartTime)     // 同排序号按计划时间
                            .Include(p => p.Bench)
                            .ToListAsync();
        }

        // 获取所有测试计划
        public async Task<List<TestPlan>> GetAllPlansAsync()
        {
            return await _db.TestPlans
                            .Include(p => p.Bench)
                            .OrderBy(p => p.BenchId)
                            .ThenByDescending(p => p.Priority)
                            .ThenBy(p => p.SortOrder)
                            .ToListAsync();
        }

        // 添加新测试计划
        public async Task AddAsync(TestPlan plan)
        {
            // 自动设置 SortOrder 为当前队列的最后一位
            var maxOrder = await _db.TestPlans
                                    .Where(p => p.BenchId == plan.BenchId)
                                    .MaxAsync(p => (int?)p.SortOrder) ?? 0;
            plan.SortOrder = maxOrder + 1;
            plan.CreatedAt = DateTime.Now;

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

                // 如果状态变为"进行中"且没有实际开始时间，自动设置
                if (newStatus == TestPlanStatus.进行中 && plan.ActualStartTime == null)
                {
                    plan.ActualStartTime = DateTime.Now;
                }

                // 如果状态变为"已完成"且没有实际结束时间，自动设置
                if (newStatus == TestPlanStatus.已完成 && plan.ActualEndTime == null)
                {
                    plan.ActualEndTime = DateTime.Now;
                }

                await _db.SaveChangesAsync();
            }
        }

        // 调整优先级
        public async Task UpdatePriorityAsync(int planId, PriorityLevel newPriority)
        {
            var plan = await _db.TestPlans.FindAsync(planId);
            if (plan != null)
            {
                plan.Priority = newPriority;
                plan.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // 调整排序（手动拖拽后）
        public async Task ReorderPlansAsync(int benchId, List<int> newOrderedIds)
        {
            for (int i = 0; i < newOrderedIds.Count; i++)
            {
                var plan = await _db.TestPlans.FindAsync(newOrderedIds[i]);
                if (plan != null && plan.BenchId == benchId)
                {
                    plan.SortOrder = i;
                }
            }
            await _db.SaveChangesAsync();
        }

        // 更新测试计划（完整编辑）
        public async Task UpdateAsync(TestPlan plan)
        {
            plan.UpdatedAt = DateTime.Now;
            _db.Entry(plan).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        // 删除测试计划
        public async Task DeleteAsync(int id)
        {
            var plan = await _db.TestPlans.FindAsync(id);
            if (plan != null)
            {
                _db.TestPlans.Remove(plan);
                await _db.SaveChangesAsync();
            }
        }
    }
}