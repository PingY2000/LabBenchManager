// Data/LabDbContext.cs
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
        public DbSet<ReportApproval> ReportApprovals => Set<ReportApproval>();  // 新增
        public DbSet<TestPlanHistory> TestPlanHistories { get; set; }
        public DbSet<BenchDocument> BenchDocuments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置 ReportApproval 和 Assignment 的关系
            modelBuilder.Entity<ReportApproval>()
                .HasOne(r => r.Assignment)
                .WithMany()
                .HasForeignKey(r => r.AssignmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}