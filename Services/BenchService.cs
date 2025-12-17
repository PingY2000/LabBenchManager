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

        /// <summary>
        /// 获取所有设备，按 ID 排序
        /// </summary>
        public async Task<List<Bench>> GetAllAsync()
        {
            return await _db.Benches.OrderBy(b => b.Id).ToListAsync();
        }

        /// <summary>
        /// 根据 ID 获取单个设备
        /// </summary>
        public async Task<Bench?> GetByIdAsync(int id)
        {
            return await _db.Benches.FindAsync(id);
        }

        /// <summary>
        /// 添加一个新设备
        /// </summary>
        public async Task AddAsync(Bench bench)
        {
            _db.Benches.Add(bench);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 根据 ID 删除一个设备
        /// </summary>
        public async Task RemoveAsync(int id)
        {
            var entity = await _db.Benches.FindAsync(id);
            if (entity != null)
            {
                _db.Benches.Remove(entity);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 更新一个现有设备。
        /// 这种实现方式更安全，因为它只更新从表单传递过来的属性，
        /// 而不会覆盖数据库中可能已被其他进程更改的其他字段。
        /// </summary>
        public async Task UpdateAsync(Bench bench)
        {
            // 1. 从数据库中找到现有的实体
            var entityToUpdate = await _db.Benches.FindAsync(bench.Id);

            if (entityToUpdate != null)
            {
                // 2. 将传入对象的属性值手动赋给从数据库中取出的实体
                //    这确保了我们只更新了"设备管理"页面关心的字段。
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

                // 注意：状态字段已移除，状态通过测试计划动态计算
                // 注意：我们没有更新 CurrentUser, Project, NextAvailableTime 等动态字段，
                // 因为"设备管理"表单并不负责编辑它们。这是一种良好的实践。

                // 3. EF Core 的更改跟踪器会自动检测到哪些属性被修改，并生成精确的 UPDATE 语句。
                await _db.SaveChangesAsync();
            }
            // 如果 entityToUpdate 为 null，可以选择抛出异常或静默处理。
        }

        /// <summary>
        /// 计算设备的当前状态（基于测试计划）
        /// </summary>
        public async Task<BenchStatus> GetBenchStatusAsync(int benchId)
        {
            var now = DateTime.Now;

            // 查找当前正在进行的测试计划
            var currentPlan = await _db.TestPlans
                .Where(p => p.BenchId == benchId
                            && p.PlannedStartTime <= now
                            && p.PlannedEndTime >= now)
                .OrderByDescending(p => p.Priority)
                .FirstOrDefaultAsync();

            if (currentPlan != null)
            {
                return currentPlan.Status switch
                {
                    TestPlanStatus.进行中 => BenchStatus.使用中,
                    TestPlanStatus.已暂停 => BenchStatus.维护中,
                    TestPlanStatus.待开始 => BenchStatus.已预定,
                    _ => BenchStatus.空闲
                };
            }

            // 检查未来的预定
            var futurePlan = await _db.TestPlans
                .Where(p => p.BenchId == benchId
                            && p.PlannedStartTime > now
                            && p.Status == TestPlanStatus.待开始)
                .OrderBy(p => p.PlannedStartTime)
                .FirstOrDefaultAsync();

            return futurePlan != null ? BenchStatus.已预定 : BenchStatus.空闲;
        }

        /// <summary>
        /// 获取所有设备及其计算出的状态
        /// </summary>
        public async Task<List<BenchWithStatus>> GetAllWithStatusAsync()
        {
            var benches = await GetAllAsync();
            var result = new List<BenchWithStatus>();

            foreach (var bench in benches)
            {
                var status = await GetBenchStatusAsync(bench.Id);
                result.Add(new BenchWithStatus
                {
                    Bench = bench,
                    Status = status
                });
            }

            return result;
        }

        /// <summary>
        /// 更新设备的动态信息（当前使用者、项目、下次可用时间）
        /// 通常由测试计划服务调用
        /// </summary>
        public async Task UpdateDynamicInfoAsync(int benchId, string? currentUser, string? project, DateTime? nextAvailableTime)
        {
            var bench = await _db.Benches.FindAsync(benchId);
            if (bench != null)
            {
                bench.CurrentUser = currentUser;
                bench.Project = project;
                bench.NextAvailableTime = nextAvailableTime;
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 根据测试计划自动更新设备的动态信息
        /// </summary>
        public async Task UpdateDynamicInfoFromPlansAsync(int benchId)
        {
            var now = DateTime.Now;

            // 查找当前正在进行的测试计划
            var currentPlan = await _db.TestPlans
                .Where(p => p.BenchId == benchId
                            && p.PlannedStartTime <= now
                            && p.PlannedEndTime >= now
                            && p.Status == TestPlanStatus.进行中)
                .OrderByDescending(p => p.Priority)
                .FirstOrDefaultAsync();

            if (currentPlan != null)
            {
                await UpdateDynamicInfoAsync(
                    benchId,
                    currentPlan.AssignedTo,
                    currentPlan.ProjectName,
                    currentPlan.PlannedEndTime
                );
            }
            else
            {
                // 查找下一个待开始的计划
                var nextPlan = await _db.TestPlans
                    .Where(p => p.BenchId == benchId
                                && p.PlannedStartTime > now
                                && p.Status == TestPlanStatus.待开始)
                    .OrderBy(p => p.PlannedStartTime)
                    .FirstOrDefaultAsync();

                if (nextPlan != null)
                {
                    await UpdateDynamicInfoAsync(
                        benchId,
                        nextPlan.AssignedTo,
                        nextPlan.ProjectName,
                        nextPlan.PlannedStartTime
                    );
                }
                else
                {
                    // 没有计划，清空动态信息
                    await UpdateDynamicInfoAsync(benchId, null, null, null);
                }
            }
        }

        /// <summary>
        /// 批量更新所有设备的动态信息
        /// 可以定期调用此方法以保持状态同步
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