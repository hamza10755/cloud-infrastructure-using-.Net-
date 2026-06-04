using System.ComponentModel.DataAnnotations;
using cloud_infrastructure.Models;

namespace cloud_infrastructure.ViewModels
{
    public class DeveloperRequestViewModel
    {
        [Required]
        [MaxLength(63)]
        [Display(Name = "Hostname")]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9]$", ErrorMessage = "Letters, digits, and hyphens only.")]
        public string Hostname { get; set; } = string.Empty;

        [Required]
        [Range(1, 128)]
        [Display(Name = "RAM (GB)")]
        public int RamGb { get; set; } = 16;

        [Required]
        [Display(Name = "Instance Size")]
        public InstanceSize InstanceSize { get; set; } = InstanceSize.Medium;

        [MaxLength(500)]
        [Display(Name = "Purpose / Justification")]
        public string? Purpose { get; set; }

        public IEnumerable<ServerInstance> RecentRequests { get; set; } = new List<ServerInstance>();
    }
}