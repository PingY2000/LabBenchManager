// Models/Bench.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    // 将 BenchStatus 枚举放在这里，或者一个单独的 Enum.cs 文件中
    public enum BenchStatus { 空闲, 使用中, 维护中, 已预定 }

    public class Bench
    {
        // --- Primary Key ---
        public int Id { get; set; }

        // --- Static Properties (from image, defining the bench) ---

        [Required(ErrorMessage = "设备名称不能为空")]
        [StringLength(100)]
        [Display(Name = "Item")]
        public string Name { get; set; } = string.Empty; // e.g., "VO36_General function hydraulic test bench"

        [StringLength(50)]
        [Display(Name = "Equipment No./Asset No.")]
        public string? EquipmentAssetNo { get; set; } // e.g., "12235422/105500"

        [StringLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; } // e.g., "DCCC-204-1F,Lab DC-IH/ECH-CN"

        [StringLength(50)]
        [Display(Name = "Test Type")]
        public string? TestType { get; set; } // e.g., "Functional test"

        [StringLength(50)]
        [Display(Name = "Test Object")]
        public string? TestObject { get; set; } // e.g., "valve"

        [Display(Name = "Quantity")]
        public int Quantity { get; set; } // e.g., 1

        [StringLength(50)]
        [Display(Name = "Norm of Working Hour")]
        public string? WorkingHoursNorm { get; set; } // e.g., "1 x 8 h x 5 day"

        [StringLength(200)]
        [Display(Name = "Basic Performance")]
        public string? BasicPerformance { get; set; } // e.g., "3 working areas A1 A2 A3"

        [StringLength(255)]
        [Display(Name = "Picture")]
        public string? PictureUrl { get; set; } // URL or path to the picture

        // --- Dynamic Properties (from original code, describing current state) ---

        // 我们从 Dashboard 页面把这些有用的字段也加进来
        [Display(Name = "Status")]
        public BenchStatus Status { get; set; } = BenchStatus.空闲;

        [StringLength(50)]
        [Display(Name = "Current User")]
        public string? CurrentUser { get; set; }

        [StringLength(100)]
        [Display(Name = "Project")]
        public string? Project { get; set; }

        [Display(Name = "Next Available Time")]
        public DateTime? NextAvailableTime { get; set; }
    }
}
// 更新数据库结构