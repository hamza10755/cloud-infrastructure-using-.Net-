using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cloud_infrastructure.Models
{
    [Table("tbl_ProvisionedServers")]
    public class ServerInstance
    {
        [Key]
        public int ServerInstanceId { get; set; }

        [Required]
        [MaxLength(63)]
        [Display(Name = "Hostname")]
        [RegularExpression(@"^[a-z0-9][a-z0-9\-]*[a-z0-9]$", ErrorMessage = "Lowercase letters, digits, and hyphens only.")]
        public string Hostname { get; set; } = string.Empty;

        [Required]
        [Range(1, 128)]
        [Display(Name = "RAM (GB)")]
        public int RamGb { get; set; }

        [Required]
        public InstanceSize InstanceSize { get; set; }
        public string? DeveloperId { get; set; }
        public Developer? Developer { get; set; }
        public ICollection<ServerSoftware> ServerSoftwares { get; set; } = new List<ServerSoftware>();
    }
}
