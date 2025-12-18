using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LabBenchManager.Services
{
    public class TestPlanHistoryService
    {
        private readonly LabDbContext _context;  // 改为 LabDbContext

        public TestPlanHistoryService(LabDbContext context)  // 改为 LabDbContext
        {
            _context = context;
        }

        public async Task<List<TestPlanHistory>> GetHistoryByPlanIdAsync(int testPlanId)
        {
            return await _context.TestPlanHistories
                .Where(h => h.TestPlanId == testPlanId)
                .OrderByDescending(h => h.ModifiedAt)
                .ToListAsync();
        }

        public async Task AddHistoryAsync(TestPlanHistory history)
        {
            _context.TestPlanHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task RecordPlanModificationAsync(
            TestPlan originalPlan,
            TestPlan modifiedPlan,
            string modifiedBy,
            string? reason = null)
        {
            // 只为确定计划状态的计划记录历史
            if (originalPlan.Status != TestPlanStatus.确定计划 &&
                modifiedPlan.Status != TestPlanStatus.确定计划)
            {
                return;
            }

            var changes = GetChangedFields(originalPlan, modifiedPlan);
            if (!changes.Any())
            {
                return; // 没有实际变更
            }

            var history = new TestPlanHistory
            {
                TestPlanId = originalPlan.Id,
                ModifiedBy = modifiedBy,
                ChangeDescription = GenerateChangeDescription(changes),
                PreviousSnapshot = JsonSerializer.Serialize(CreateSnapshot(originalPlan)),
                NewSnapshot = JsonSerializer.Serialize(CreateSnapshot(modifiedPlan)),
                ChangedFields = JsonSerializer.Serialize(changes),
                Reason = reason
            };

            await AddHistoryAsync(history);
        }

        private Dictionary<string, (object? OldValue, object? NewValue)> GetChangedFields(
            TestPlan original, TestPlan modified)
        {
            var changes = new Dictionary<string, (object?, object?)>();

            if (original.ProjectName != modified.ProjectName)
                changes["项目名称"] = (original.ProjectName, modified.ProjectName);

            if (original.Description != modified.Description)
                changes["描述"] = (original.Description, modified.Description);

            if (original.Status != modified.Status)
                changes["状态"] = (original.Status.ToString(), modified.Status.ToString());

            if (original.AssignedTo != modified.AssignedTo)
                changes["负责人"] = (original.AssignedTo, modified.AssignedTo);

            // 比较日期
            var originalDates = original.GetScheduledDates().OrderBy(d => d).ToList();
            var modifiedDates = modified.GetScheduledDates().OrderBy(d => d).ToList();

            if (!originalDates.SequenceEqual(modifiedDates))
            {
                changes["测试日期"] = (
                    string.Join(", ", originalDates.Select(d => d.ToString("yyyy-MM-dd"))),
                    string.Join(", ", modifiedDates.Select(d => d.ToString("yyyy-MM-dd")))
                );
            }

            return changes;
        }

        private string GenerateChangeDescription(
            Dictionary<string, (object? OldValue, object? NewValue)> changes)
        {
            var descriptions = changes.Select(c =>
                $"{c.Key}: {c.Value.OldValue} → {c.Value.NewValue}");
            return string.Join("; ", descriptions);
        }

        private object CreateSnapshot(TestPlan plan)
        {
            return new
            {
                plan.ProjectName,
                plan.Description,
                Status = plan.Status.ToString(),
                plan.AssignedTo,
                ScheduledDates = plan.GetScheduledDates().Select(d => d.ToString("yyyy-MM-dd")),
                plan.SampleQuantity,
                plan.RequestedBy
            };
        }
    }
}