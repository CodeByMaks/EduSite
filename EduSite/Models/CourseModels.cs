using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSite.Models
{
    public class UserCourse
    {
        public UserCourse()
        {
            UserId = string.Empty;
            Progress = 0;
        }

        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsCompleted { get; set; }
        public int Progress { get; set; }
    }

    public class CourseModule
    {
        public CourseModule()
        {
            Title = string.Empty;
            Description = string.Empty;
            Contents = new List<ModuleContent>();
            Order = 0;
        }

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public virtual ICollection<ModuleContent> Contents { get; set; }
    }

    public class ModuleContent
    {
        public ModuleContent()
        {
            Title = string.Empty;
            Description = string.Empty;
            ContentType = string.Empty;
            ContentUrl = string.Empty;
            TextContent = string.Empty;
            CompletedByUsers = new List<CompletedContent>();
        }

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ContentType { get; set; }

        [Required]
        public string ContentUrl { get; set; }

        [Required]
        public string TextContent { get; set; }

        public int ModuleId { get; set; }
        public virtual CourseModule Module { get; set; }

        public virtual ICollection<CompletedContent> CompletedByUsers { get; set; }
    }

    public class CompletedContent
    {
        public CompletedContent()
        {
            UserId = string.Empty;
        }

        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int ContentId { get; set; }
        public virtual ModuleContent Content { get; set; }

        public DateTime CompletedDate { get; set; }
    }
}
