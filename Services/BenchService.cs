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
                //    这确保了我们只更新了“设备管理”页面关心的字段。
                entityToUpdate.Name = bench.Name;
                entityToUpdate.EquipmentAssetNo = bench.EquipmentAssetNo;
                entityToUpdate.Location = bench.Location;
                entityToUpdate.TestType = bench.TestType;
                entityToUpdate.TestObject = bench.TestObject;
                entityToUpdate.Quantity = bench.Quantity;
                entityToUpdate.WorkingHoursNorm = bench.WorkingHoursNorm;
                entityToUpdate.BasicPerformance = bench.BasicPerformance;
                entityToUpdate.PictureUrl = bench.PictureUrl;
                entityToUpdate.Status = bench.Status;

                // 注意：我们没有更新 CurrentUser, Project, NextAvailableTime 等动态字段，
                // 因为“设备管理”表单并不负责编辑它们。这是一种良好的实践。

                // 3. EF Core 的更改跟踪器会自动检测到哪些属性被修改，并生成精确的 UPDATE 语句。
                await _db.SaveChangesAsync();
            }
            // 如果 entityToUpdate 为 null，可以选择抛出异常或静默处理。
        }
    }
}