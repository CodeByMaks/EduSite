using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSite.Models
{
    public class Course
    {
        public Course()
        {
            Title = string.Empty;
            Description = string.Empty;
            Author = string.Empty;
            Category = string.Empty;
            Duration = string.Empty;
            Difficulty = string.Empty;
            ImageUrl = string.Empty;
            Modules = new List<CourseModule>();
            EnrolledUsers = new List<UserCourse>();
            CreatedDate = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string Difficulty { get; set; }

        public bool IsFree { get; set; }

        [Range(0, 1000)]
        public decimal Price { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        public string ImageUrl { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public virtual List<CourseModule> Modules { get; set; }
        public virtual List<UserCourse> EnrolledUsers { get; set; }

        public static Course GenerateRandom()
        {
            var course = new Course
            {
                Title = "Новый курс",
                Category = "Программирование",
                Description = "Описание курса",
                Author = "Автор",
                Duration = "4 недели",
                Difficulty = "Начинающий",
                IsFree = true,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                ImageUrl = "/images/courses-hero.svg",
                Modules = new List<CourseModule>(),
                EnrolledUsers = new List<UserCourse>()
            };

            return course;
        }
    }
}
