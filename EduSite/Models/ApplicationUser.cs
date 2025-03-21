using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSite.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        public string? ProfilePictureUrl { get; set; }

        public virtual ICollection<UserCourse> EnrolledCourses { get; set; }

        [NotMapped]
        public IList<string> Roles { get; set; }

        public ApplicationUser()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            EnrolledCourses = new List<UserCourse>();
            Roles = new List<string>();
        }
    }
}
