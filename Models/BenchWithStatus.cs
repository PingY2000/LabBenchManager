// Models/BenchWithStatus.cs
namespace LabBenchManager.Models
{
    /// <summary>
    /// 设备及其计算出的状态
    /// 用于在 UI 中显示设备的实时状态
    /// </summary>
    public class BenchWithStatus
    {
        public Bench Bench { get; set; } = null!;
        public BenchStatus Status { get; set; }
    }
}