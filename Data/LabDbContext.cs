//Data/LabDbContext.cs
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;
namespace LabBenchManager.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Bench> Benches => Set<Bench>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<TestPlan> TestPlans => Set<TestPlan>();
    }
}