using LabBenchManager.Models;
using LabBenchManager.Data;
using Microsoft.EntityFrameworkCore;
namespace LabBenchManager.Services
{
    public class BenchService 
    {
        private readonly LabDbContext _db;
        public BenchService(LabDbContext db) { _db = db; }
        public async Task<List<Bench>> GetAllAsync() => await _db.Benches.OrderBy(b => b.Id).ToListAsync(); public async Task AddAsync(Bench bench) { _db.Benches.Add(bench); await _db.SaveChangesAsync(); }
        public async Task RemoveAsync(int id) { var entity = await _db.Benches.FindAsync(id); if (entity != null) { _db.Benches.Remove(entity); await _db.SaveChangesAsync(); } }
        public async Task UpdateAsync(Bench bench) { _db.Benches.Update(bench); await _db.SaveChangesAsync(); }
    }
}