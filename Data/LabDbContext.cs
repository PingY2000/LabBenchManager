using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;
namespace LabBenchManager.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Bench> Benches => Set<Bench>();
    }
}