// Models/ApplicationUser.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public static class AppRoles // 使用静态类来定义角色常量，避免魔法字符串
    {
        public const string Admin = "Admin";
        public const string TestEngineer = "TestEngineer";
        public const string Requester = "Requester";
    }

    public class ApplicationUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NtAccount { get; set; } = string.Empty; // e.g., "DOMAIN\username"

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty; // 直接存储角色名，如 "Admin"

        // (可选)可以添加其他用户信息，如DisplayName, Email等
        [StringLength(100)]
        public string? DisplayName { get; set; }
    }
}