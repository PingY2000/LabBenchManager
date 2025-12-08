using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public class Bench
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Description { get; set; }
        // 添加其他需要的属性
    }
}
/*
namespace LabBenchManager.Models
{
    public enum BenchStatus { Idle, InUse, Maintenance, Offline }
    public class Bench { public int Id { get; set; } [Required, StringLength(100)] public string Name { get; set; } = ""; [StringLength(100)] public string? Location { get; set; } public BenchStatus Status { get; set; } = BenchStatus.Idle; }
}*/