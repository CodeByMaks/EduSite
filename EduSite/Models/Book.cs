using System;
using System.ComponentModel.DataAnnotations;

namespace EduSite.Models
{
    public class Book
    {
        public Book()
        {
            Title = string.Empty;
            Author = string.Empty;
            Description = string.Empty;
            FilePath = string.Empty;
            ISBN = string.Empty;
            Language = string.Empty;
            Publisher = string.Empty;
            DateAdded = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Author { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Price ($)")]
        public decimal Price { get; set; }

        [Required]
        public bool IsFree { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string? CoverImagePath { get; set; }

        [Required]
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        [Display(Name = "Page Count")]
        [Range(1, 10000)]
        public int PageCount { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Publisher { get; set; }

        [Display(Name = "Download Count")]
        public int DownloadCount { get; set; }

        [Display(Name = "File Size")]
        public string? FileSize { get; set; }

        [Display(Name = "File Format")]
        public string FileFormat { get; set; } = "PDF";

        public string? Category { get; set; }

        [Display(Name = "Publication Year")]
        public int PublicationYear { get; set; }

        public string? Format { get; set; }
    }
}
