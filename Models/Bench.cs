// Models/Bench.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    // 将 BenchStatus 枚举放在这里，或者一个单独的 Enum.cs 文件中
    public enum BenchStatus { 空闲, 使用中, 维护中, 已预定 }

    public class Bench
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "设备名称不能为空")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        // 我们从 Dashboard 页面把这些有用的字段也加进来
        public BenchStatus Status { get; set; } = BenchStatus.空闲;

        [StringLength(50)]
        public string? CurrentUser { get; set; }

        [StringLength(100)]
        public string? Project { get; set; }

        public DateTime? NextAvailableTime { get; set; }
    }
}