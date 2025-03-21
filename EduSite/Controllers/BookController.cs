using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EduSite.Data;
using EduSite.Models;
using EduSite.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace EduSite.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BookController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Book
        public async Task<IActionResult> Index(string searchString)
        {
            var books = from b in _context.Books
                       select new BookViewModel
                       {
                           Id = b.Id,
                           Title = b.Title,
                           Author = b.Author,
                           Description = b.Description,
                           CoverImageUrl = b.CoverImagePath ?? string.Empty,
                           DownloadUrl = b.FilePath,
                           Category = b.Category ?? string.Empty,
                           PublicationYear = b.PublicationYear,
                           Format = b.Format ?? string.Empty,
                           Price = b.Price,
                           IsFree = b.IsFree,
                           ISBN = b.ISBN ?? string.Empty,
                           PageCount = b.PageCount
                       };

            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString) ||
                                       s.Author.Contains(searchString) ||
                                       s.Description.Contains(searchString));
            }

            return View(await books.ToListAsync());
        }

        // GET: Book/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Book/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] BookViewModel bookViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(bookViewModel);
                }

                var book = new Book
                {
                    Title = bookViewModel.Title,
                    Description = bookViewModel.Description,
                    Author = bookViewModel.Author,
                    IsFree = bookViewModel.IsFree,
                    Price = bookViewModel.IsFree ? 0 : bookViewModel.Price,
                    ISBN = bookViewModel.ISBN,
                    PageCount = bookViewModel.PageCount,
                    Language = bookViewModel.Language,
                    Publisher = bookViewModel.Publisher,
                    DateAdded = DateTime.UtcNow,
                    DownloadCount = 0,
                    CoverImagePath = "/images/default-book-cover.jpg"
                };

                // Обработка файла книги
                if (bookViewModel.BookFile != null && bookViewModel.BookFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "books");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(bookViewModel.BookFile.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(bookViewModel.BookFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookViewModel.BookFile.CopyToAsync(fileStream);
                    }

                    book.FilePath = $"/uploads/books/{uniqueFileName}";
                    book.FileSize = $"{(bookViewModel.BookFile.Length / 1024.0 / 1024.0):F2} MB";
                    book.FileFormat = Path.GetExtension(bookViewModel.BookFile.FileName).TrimStart('.').ToUpper();
                }

                // Обработка изображения обложки
                if (bookViewModel.CoverImage != null && bookViewModel.CoverImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(bookViewModel.CoverImage.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(bookViewModel.CoverImage.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookViewModel.CoverImage.CopyToAsync(fileStream);
                    }

                    book.CoverImagePath = $"/uploads/covers/{uniqueFileName}";
                }

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Книга успешно добавлена";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Произошла ошибка при создании книги");
                return View(bookViewModel);
            }
        }

        // Метод поиска книг для живого поиска
        [HttpGet]
        [Route("Book/Search")]
        public async Task<IActionResult> Search(string query)
        {
            var books = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                books = books.Where(b => b.Title.Contains(query) ||
                                       b.Description.Contains(query) ||
                                       b.Author.Contains(query) ||
                                       b.Category.Contains(query));
            }

            var result = await books.Select(b => new BookViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Description = b.Description,
                CoverImageUrl = b.CoverImagePath ?? string.Empty,
                DownloadUrl = b.FilePath,
                Category = b.Category ?? string.Empty,
                PublicationYear = b.PublicationYear,
                Format = b.Format ?? string.Empty,
                Price = b.Price,
                IsFree = b.IsFree,
                ISBN = b.ISBN ?? string.Empty,
                PageCount = b.PageCount
            }).ToListAsync();

            return Json(result);
        }

        // GET: api/Book/Search
        [HttpGet]
        [Route("api/Book/Search")]
        public async Task<IActionResult> SearchApi(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<Book>());

            var books = await _context.Books
                .Where(b => b.Title.Contains(query) ||
                           b.Author.Contains(query) ||
                           b.Description.Contains(query))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Description,
                    b.CoverImagePath,
                    b.Price,
                    b.IsFree,
                    b.FileSize,
                    b.DownloadCount
                })
                .Take(10)
                .ToListAsync();

            return Json(books);
        }

        // GET: Book/ConfirmPurchase/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            if (book.IsFree)
            {
                return RedirectToAction(nameof(Download), new { id });
            }

            return View(book);
        }

        // POST: Book/ConfirmPurchase/5
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ConfirmPurchase(int id, bool confirm)
        {
            if (!confirm)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            // После подтверждения сразу скачиваем книгу
            return await Download(id);
        }

        // GET: Book/Download/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            // Убираем начальный слеш из пути к файлу
            var relativePath = book.FilePath.TrimStart('/');
            var filePath = Path.Combine(_environment.WebRootPath, relativePath);
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            book.DownloadCount++;
            await _context.SaveChangesAsync();

            var fileName = book.Title.Replace(" ", "_") + Path.GetExtension(book.FilePath);
            var contentType = GetContentType(book.FileFormat);
            return PhysicalFile(filePath, contentType, fileName);
        }

        private string GetContentType(string fileFormat)
        {
            return fileFormat.ToLower() switch
            {
                "pdf" => "application/pdf",
                "epub" => "application/epub+zip",
                "mobi" => "application/x-mobipocket-ebook",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        // GET: Book/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                // Удаляем файл PDF
                if (!string.IsNullOrEmpty(book.FilePath))
                {
                    var pdfPath = Path.Combine(_environment.WebRootPath, book.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                    }
                }

                // Удаляем обложку
                if (!string.IsNullOrEmpty(book.CoverImagePath))
                {
                    var coverPath = Path.Combine(_environment.WebRootPath, book.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(coverPath))
                    {
                        System.IO.File.Delete(coverPath);
                    }
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
