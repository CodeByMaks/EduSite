using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduSite.Models.ViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Author { get; set; }

        [Required]
        [Display(Name = "Категория")]
        public string Category { get; set; }

        [Required]
        public string Description { get; set; }

        public int PublicationYear { get; set; }

        public string Format { get; set; }

        public string CoverImageUrl { get; set; }

        public string DownloadUrl { get; set; }

        [Display(Name = "Book File")]
        public IFormFile? BookFile { get; set; }

        [Display(Name = "Cover Image")]
        public IFormFile? CoverImage { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Display(Name = "Free Book")]
        public bool IsFree { get; set; }

        [Required]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Required]
        [Display(Name = "Page Count")]
        [Range(1, 10000)]
        public int PageCount { get; set; }

        [Required]
        [Display(Name = "Language")]
        public string Language { get; set; }

        [Required]
        [Display(Name = "Publisher")]
        public string Publisher { get; set; }

        [Display(Name = "Downloads")]
        public int DownloadCount { get; set; } = 0;

        public BookViewModel()
        {
            Title = string.Empty;
            Author = string.Empty;
            Description = string.Empty;
            Category = string.Empty;
            Format = string.Empty;
            CoverImageUrl = string.Empty;
            DownloadUrl = string.Empty;
            ISBN = string.Empty;
            Language = string.Empty;
            Publisher = string.Empty;
        }
    }
}
