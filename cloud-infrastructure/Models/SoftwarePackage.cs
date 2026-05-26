using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cloud_infrastructure.Models
{
    public class SoftwarePackage
    {
        [Key]
        public int SoftwarePackageId { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "Package Name")]
        public string PackageName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Version { get; set; } = string.Empty;

        public ICollection<ServerSoftware> ServerSoftwares { get; set; } = new List<ServerSoftware>();
    }
}
