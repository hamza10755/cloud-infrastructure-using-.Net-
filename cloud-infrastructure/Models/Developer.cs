using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace cloud_infrastructure.Models
{
    public class Developer : IdentityUser
    {
        [Key]
        public int DeveloperId { get; set; }

        [Required]
        [MaxLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [Display(Name = "Work Email")]
        public string WorkEmail { get; set; } = string.Empty;

        public ICollection<ServerInstance> ServerInstances { get; set; } = new List<ServerInstance>();
    }
}
