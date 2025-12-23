// Models/BenchDocument.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public class BenchDocument
    {
        public int Id { get; set; }

        public int BenchId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "文件名")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "文件路径")]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "文件类型")]
        public string? FileType { get; set; } // PDF, Word, Excel, etc.

        [Display(Name = "文件大小(字节)")]
        public long FileSize { get; set; }

        [StringLength(200)]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "上传时间")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "上传者")]
        public string? UploadedBy { get; set; }

        // Navigation Property
        public Bench? Bench { get; set; }
    }
}