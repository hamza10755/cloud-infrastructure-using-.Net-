using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cloud_infrastructure.Models
{
    public class ApplicationDbContext
    {
        [Key]
        public int DeveloperId { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [Display(Name = "Work Email")]
        public string WorkEmail { get; set; }

        public ICollection<ServerInstance> ServerInstances { get; set; } = new List<ServerInstance>();
    }
}
