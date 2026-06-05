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

        [Required]
        [Display(Name = "Operating System")]
        public string OperatingSystem { get; set; } = "Ubuntu 24.04 LTS";

        [MaxLength(500)]
        [Display(Name = "Purpose / Justification")]
        public string? Purpose { get; set; }

        public int? EditId { get; set; }

        public IEnumerable<ServerInstance> RecentRequests { get; set; } = new List<ServerInstance>();

        public static readonly string[] OperatingSystems =
        [
            "Ubuntu 22.04 LTS",
            "Ubuntu 24.04 LTS",
            "Debian 12",
            "Rocky Linux 9",
            "Windows Server 2022",
            "Windows Server 2019"
        ];
    }
}