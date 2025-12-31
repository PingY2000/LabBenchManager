// Services/BenchService.cs
using DocumentFormat.OpenXml.InkML;
using LabBenchManager.Data;
using LabBenchManager.Models;
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
            return await _db.Benches
                .OrderBy(b => b.SortOrder) // 🔑 按排序字段排序
                .ThenBy(b => b.Id)
                .ToListAsync();
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
            bool hasPlanToday = plans.Any(p => p.GetScheduledDates().Contains(today));
            if (hasPlanToday)
            {
                return BenchStatus.使用中;
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
            var today = DateTime.Today; // 提前定义，避免每次循环创建

            var allPlans = await _db.TestPlans.ToListAsync();
            var benches = await GetAllAsync();
            var result = new List<BenchWithStatus>();

            foreach (var bench in benches)
            {
                var plansForBench = allPlans.Where(p => p.BenchId == bench.Id).ToList();

                // 1. 只要今天有任何计划（不管状态），就是“使用中”
                if (plansForBench.Any(p => p.GetScheduledDates().Contains(today)))
                {
                    result.Add(new BenchWithStatus
                    {
                        Bench = bench,
                        Status = BenchStatus.使用中
                    });
                }
                // 2. 否则，检查未来是否有任何预定（不管状态）
                else if (plansForBench.Any(p => p.GetScheduledDates().Any(d => d > today)))
                {
                    result.Add(new BenchWithStatus
                    {
                        Bench = bench,
                        Status = BenchStatus.已预定
                    });
                }
                // 3. 完全没安排
                else
                {
                    result.Add(new BenchWithStatus
                    {
                        Bench = bench,
                        Status = BenchStatus.空闲
                    });
                }
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

            // 查找今天状态为"进行中"的计划
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
                // 如果今天没有"进行中"的计划，则清空动态信息
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

        // ==================== 文档管理方法 ====================

        /// <summary>
        /// 获取指定设备的所有文档
        /// </summary>
        public async Task<List<BenchDocument>> GetDocumentsByBenchIdAsync(int benchId)
        {
            return await _db.BenchDocuments
                .Where(d => d.BenchId == benchId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 添加新文档
        /// </summary>
        public async Task<BenchDocument> AddDocumentAsync(BenchDocument document)
        {
            _db.BenchDocuments.Add(document);
            await _db.SaveChangesAsync();
            return document;
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        public async Task DeleteDocumentAsync(int documentId)
        {
            var document = await _db.BenchDocuments.FindAsync(documentId);
            if (document != null)
            {
                _db.BenchDocuments.Remove(document);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 根据ID获取文档
        /// </summary>
        public async Task<BenchDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _db.BenchDocuments.FindAsync(documentId);
        }

        /// <summary>
        /// 获取设备及其文档
        /// </summary>
        public async Task<Bench?> GetBenchWithDocumentsAsync(int benchId)
        {
            return await _db.Benches
                .Include(b => b.Documents)
                .FirstOrDefaultAsync(b => b.Id == benchId);
        }


        public async Task UpdateSortOrderAsync(int benchId, int newOrder)
        {
            var bench = await _db.Benches.FindAsync(benchId);
            if (bench != null)
            {
                bench.SortOrder = newOrder;
                await _db.SaveChangesAsync();
            }
        }

       

        public async Task MoveUpAsync(int benchId)
        {
            var benches = await _db.Benches.OrderBy(b => b.SortOrder).ToListAsync();
            var currentIndex = benches.FindIndex(b => b.Id == benchId);

            if (currentIndex > 0)
            {
                var temp = benches[currentIndex].SortOrder;
                benches[currentIndex].SortOrder = benches[currentIndex - 1].SortOrder;
                benches[currentIndex - 1].SortOrder = temp;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MoveDownAsync(int benchId)
        {
            var benches = await _db.Benches.OrderBy(b => b.SortOrder).ToListAsync();
            var currentIndex = benches.FindIndex(b => b.Id == benchId);

            if (currentIndex < benches.Count - 1)
            {
                var temp = benches[currentIndex].SortOrder;
                benches[currentIndex].SortOrder = benches[currentIndex + 1].SortOrder;
                benches[currentIndex + 1].SortOrder = temp;
                await _db.SaveChangesAsync();
            }
        }
    }
}