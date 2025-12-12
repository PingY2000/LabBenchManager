// Models/Assignment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabBenchManager.Models
{
    public enum AssignmentStatus { 待审批, 待分配, 已拒绝, 进行中, 已完成 ,未开始}

    public enum TestStage { 第一轮, 第二轮, 第三轮, 第四轮, 其他轮次 }

    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "申请人姓名不能为空")]
        [StringLength(50)]
        [Display(Name = "申请人姓名")]
        public string ApplicantName { get; set; } = "";

        [Required(ErrorMessage = "项目名称不能为空")]
        [StringLength(100)]
        [Display(Name = "项目名称")]
        public string ProjectName { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "部门/单位")]
        public string? Department { get; set; } = "";

        [StringLength(20)]
        [Display(Name = "联系电话")]
        public string? ContactPhone { get; set; } = "";

        [StringLength(100)]
        [Display(Name = "电子邮箱")]
        public string? ContactEmail { get; set; } = "";

        // 可选的外键关联到 Bench
        [Display(Name = "测试设备")]
        public int? BenchId { get; set; } // 改为可空类型

        [ForeignKey("BenchId")]
        public virtual Bench? Bench { get; set; }

        // 新增：可选的外键关联到 TestPlan
        [Display(Name = "测试计划")]
        public int? TestPlanId { get; set; }

        [ForeignKey("TestPlanId")]
        public virtual TestPlan? TestPlan { get; set; }

        public DateTime RequestTime { get; set; }

        // 时间规划
        [Required(ErrorMessage = "必须指定预计交样时间")]
        [Display(Name = "预计交样时间")]
        public DateTime EstimatedSampleTime { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "必须指定期望完成时间")]
        [Display(Name = "期望完成时间")]
        public DateTime DesiredCompletionTime { get; set; } = DateTime.Today.AddDays(5);

        // 样品信息
        [Required(ErrorMessage = "样品数量必须填写")]
        [Range(1, 1000, ErrorMessage = "样品数量必须在1-1000之间")]
        [Display(Name = "样品数量")]
        public int SampleQuantity { get; set; } = 1;

        [StringLength(200)]
        [Display(Name = "样品规格/型号")]
        public string? SampleSpecification { get; set; }

        [StringLength(100)]
        [Display(Name = "样品批号")]
        public string? SampleBatchNo { get; set; }

        [StringLength(500)]
        [Display(Name = "样品特殊要求")]
        public string? SampleRequirements { get; set; }

        // 测试需求详情
        [Required(ErrorMessage = "测试内容不能为空")]
        [StringLength(2000)]
        [Display(Name = "测试内容")]
        public string TestContent { get; set; } = "";

        [StringLength(500)]
        [Display(Name = "测试标准/依据")]
        public string? TestStandard { get; set; } = "";

        [StringLength(1000)]
        [Display(Name = "测试参数/条件")]
        public string? TestParameters { get; set; }

        [Display(Name = "测试阶段")]
        public TestStage Stage { get; set; } = TestStage.第一轮;

        [StringLength(100)]
        [Display(Name = "其他阶段说明")]
        public string? StageDescription { get; set; }

        // 特殊要求
        [StringLength(1000)]
        [Display(Name = "特殊要求")]
        public string? SpecialRequirements { get; set; }

        [Display(Name = "是否需要加急")]
        public bool IsUrgent { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "加急原因")]
        public string? UrgentReason { get; set; }

        // 状态和备注
        public AssignmentStatus Status { get; set; } = AssignmentStatus.待审批;

        [StringLength(1000)]
        [Display(Name = "备注说明")]
        public string? Notes { get; set; }
    }
}