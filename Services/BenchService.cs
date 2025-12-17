// Services/BenchService.cs
using LabBenchManager.Models;
using LabBenchManager.Data;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class BenchService
    {
        private readonly LabDbContext _db;

        public BenchService(LabDbContext db)
        {
            _db = db;
        }

        // 获取所有设备
        public async Task<List<Bench>> GetAllAsync()
        {
            return await _db.Benches.OrderBy(b => b.Id).ToListAsync();
        }

        // 根据ID获取单个设备
        public async Task<Bench?> GetByIdAsync(int id)
        {
            return await _db.Benches.FindAsync(id);
        }

        // 添加新设备
        public async Task AddAsync(Bench bench)
        {
            _db.Benches.Add(bench);
            await _db.SaveChangesAsync();
        }

        // 删除设备
        public async Task RemoveAsync(int id)
        {
            var entity = await _db.Benches.FindAsync(id);
            if (entity != null)
            {
                _db.Benches.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        // 更新设备基本信息
        public async Task UpdateAsync(Bench bench)
        {
            var entityToUpdate = await _db.Benches.FindAsync(bench.Id);
            if (entityToUpdate != null)
            {
                // 手动更新从表单传递的字段
                entityToUpdate.Name = bench.Name;
                entityToUpdate.EquipmentNo = bench.EquipmentNo;
                entityToUpdate.AssetNo = bench.AssetNo;
                entityToUpdate.Location = bench.Location;
                entityToUpdate.TestType = bench.TestType;
                entityToUpdate.TestObject = bench.TestObject;
                entityToUpdate.Quantity = bench.Quantity;
                entityToUpdate.WorkingHoursNorm = bench.WorkingHoursNorm;
                entityToUpdate.BasicPerformanceAndConfiguration = bench.BasicPerformanceAndConfiguration;
                entityToUpdate.PictureUrl = bench.PictureUrl;

                // 注意：动态字段 CurrentUser, Project 等不在这里更新
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 计算设备的当前状态（基于以天为单位的测试计划）
        /// </summary>
        public async Task<BenchStatus> GetBenchStatusAsync(int benchId)
        {
            var today = DateTime.Today;

            // 由于无法在数据库中直接查询字符串解析后的日期，我们先将相关计划加载到内存中
            var plans = await _db.TestPlans
                                 .Where(p => p.BenchId == benchId)
                                 .ToListAsync();

            // 1. 检查今天是否有计划
            var activePlanToday = plans.FirstOrDefault(p => p.GetScheduledDates().Contains(today));

            if (activePlanToday != null)
            {
                // 如果今天有计划，则根据计划的状态决定设备状态
                return activePlanToday.Status switch
                {
                    _ => BenchStatus.空闲 // 已完成或已取消的计划意味着设备今天空闲
                };
            }

            // 2. 如果今天没有计划，检查未来是否有预定
            var hasFutureBooking = plans.Any(p => p.GetScheduledDates().Any(d => d > today));
            if (hasFutureBooking)
            {
                return BenchStatus.已预定;
            }

            // 3. 如果既没有今天的计划，也没有未来的预定，则设备为空闲
            return BenchStatus.空闲;
        }

        /// <summary>
        /// 获取所有设备及其计算出的状态
        /// </summary>
        public async Task<List<BenchWithStatus>> GetAllWithStatusAsync()
        {
            // 为了优化性能，一次性加载所有计划
            var allPlans = await _db.TestPlans.ToListAsync();
            var benches = await GetAllAsync();
            var result = new List<BenchWithStatus>();

            foreach (var bench in benches)
            {
                var today = DateTime.Today;
                var plansForBench = allPlans.Where(p => p.BenchId == bench.Id).ToList();

                // --- 重复 GetBenchStatusAsync 的逻辑以避免多次数据库调用 ---
                var status = BenchStatus.空闲;
                var activePlanToday = plansForBench.FirstOrDefault(p => p.GetScheduledDates().Contains(today));

                if (activePlanToday != null)
                {
                    status = activePlanToday.Status switch
                    {
                        _ => BenchStatus.空闲
                    };
                }
                else if (plansForBench.Any(p => p.GetScheduledDates().Any(d => d > today)))
                {
                    status = BenchStatus.已预定;
                }
                // --- 逻辑结束 ---

                result.Add(new BenchWithStatus
                {
                    Bench = bench,
                    Status = status
                });
            }
            return result;
        }

        /// <summary>
        /// 根据测试计划自动更新设备的动态信息 (CurrentUser, Project)
        /// </summary>
        public async Task UpdateDynamicInfoFromPlansAsync(int benchId)
        {
            var bench = await _db.Benches.FindAsync(benchId);
            if (bench == null) return;

            var today = DateTime.Today;

            // 查找今天状态为“进行中”的计划
            var plans = await _db.TestPlans
                                 .Where(p => p.BenchId == benchId && p.Status == TestPlanStatus.确定计划)
                                 .ToListAsync();

            var currentPlan = plans.FirstOrDefault(p => p.GetScheduledDates().Contains(today));

            if (currentPlan != null)
            {
                // 如果找到，则更新设备上的动态信息
                bench.CurrentUser = currentPlan.AssignedTo;
                bench.Project = currentPlan.ProjectName;
            }
            else
            {
                // 如果今天没有“进行中”的计划，则清空动态信息
                bench.CurrentUser = null;
                bench.Project = null;
            }

            // 移除了对 NextAvailableTime 的更新
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 批量更新所有设备的动态信息
        /// </summary>
        public async Task UpdateAllDynamicInfoAsync()
        {
            var benches = await GetAllAsync();
            foreach (var bench in benches)
            {
                await UpdateDynamicInfoFromPlansAsync(bench.Id);
            }
        }
    }
}