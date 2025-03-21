using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduSite.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Категория")]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string Difficulty { get; set; }

        public bool IsFree { get; set; } = true;

        [Range(0, 9999.99)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Course Image")]
        public IFormFile? ImageFile { get; set; }
        public string? ImageUrl { get; set; }

        [Display(Name = "Количество учащихся")]
        public int EnrolledCount { get; set; }
    }
}
