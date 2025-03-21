using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSite.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using EduSite.Data;
using EduSite.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace EduSite.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ApplicationDbContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Course
        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                _logger.LogInformation("Loading courses from database...");
                
                // Создаем случайные курсы, если их нет
                if (!await _context.Courses.AnyAsync())
                {
                    _logger.LogInformation("No courses found, generating random courses...");
                    try
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var course = Course.GenerateRandom();
                            await _context.Courses.AddAsync(course);
                        }
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Successfully generated and saved random courses");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error generating random courses");
                        throw;
                    }
                }

                // Загружаем курсы с фильтрацией по поисковому запросу
                var coursesQuery = _context.Courses
                    .Include(c => c.Modules)
                    .Include(c => c.EnrolledUsers)
                    .AsNoTracking();

                if (!String.IsNullOrEmpty(searchString))
                {
                    _logger.LogInformation($"Filtering courses by search string: {searchString}");
                    coursesQuery = coursesQuery.Where(c => 
                        c.Title.Contains(searchString) ||
                        c.Description.Contains(searchString) ||
                        c.Author.Contains(searchString));
                }

                var courses = await coursesQuery.ToListAsync();

                if (courses == null)
                {
                    _logger.LogWarning("No courses found in database");
                    return View(new List<Course>());
                }

                _logger.LogInformation($"Successfully loaded {courses.Count} courses");
                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading courses");
                return StatusCode(500, new { message = "Error loading courses. Please try again later." });
            }
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Check if the user is enrolled in this course
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.IsEnrolled = await _context.UserCourses
                    .AnyAsync(uc => uc.UserId == userId && uc.CourseId == id);
            }
            else
            {
                ViewBag.IsEnrolled = false;
            }

            return View(course);
        }

        // GET: Course/Create
        [Authorize(Policy = "RequireAdminRole")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Create([FromForm] CourseViewModel courseViewModel)
        {
            try
            {
                _logger.LogInformation("Create method called");

                if (!ModelState.IsValid)
                {
                    return View(courseViewModel);
                }

                var course = new Course
                {
                    Title = courseViewModel.Title,
                    Category = courseViewModel.Category,
                    Description = courseViewModel.Description,
                    Author = courseViewModel.Author,
                    Duration = courseViewModel.Duration,
                    Difficulty = courseViewModel.Difficulty,
                    IsFree = courseViewModel.IsFree,
                    Price = courseViewModel.IsFree ? 0 : courseViewModel.Price,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    ImageUrl = "/images/courses-hero.svg"
                };

                if (courseViewModel.ImageFile != null)
                {
                    var fileName = Path.GetFileName(courseViewModel.ImageFile.FileName);
                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "courses");
                    
                    Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await courseViewModel.ImageFile.CopyToAsync(fileStream);
                    }

                    course.ImageUrl = $"/images/courses/{uniqueFileName}";
                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Курс успешно создан";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError("", "Произошла ошибка при создании курса");
                return View(courseViewModel);
            }
        }

        // GET: Course/Edit/5
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var courseViewModel = new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Category = course.Category,
                Description = course.Description,
                Author = course.Author,
                Duration = course.Duration,
                Difficulty = course.Difficulty,
                IsFree = course.IsFree,
                Price = course.Price,
                ImageUrl = course.ImageUrl
            };

            return View(courseViewModel);
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Category,Description,Author,Duration,Difficulty,IsFree,Price,ImageFile")] CourseViewModel courseViewModel)
        {
            _logger.LogInformation($"Edit method called for course ID: {id}");
            _logger.LogInformation($"CourseViewModel data: Title={courseViewModel.Title}, IsFree={courseViewModel.IsFree}, Price={courseViewModel.Price}");

            if (id != courseViewModel.Id)
            {
                _logger.LogInformation($"ID mismatch: URL id={id}, ViewModel id={courseViewModel.Id}");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var course = await _context.Courses
                        .Include(c => c.Modules)
                        .Include(c => c.EnrolledUsers)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (course == null)
                    {
                        _logger.LogInformation($"Course with ID {id} not found");
                        return NotFound();
                    }

                    _logger.LogInformation($"Found course: {course.Title}");
                    _logger.LogInformation($"Updating course properties...");

                    course.Title = courseViewModel.Title;
                    course.Category = courseViewModel.Category;
                    course.Description = courseViewModel.Description;
                    course.Author = courseViewModel.Author;
                    course.Duration = courseViewModel.Duration;
                    course.Difficulty = courseViewModel.Difficulty;
                    course.IsFree = courseViewModel.IsFree;
                    course.Price = courseViewModel.IsFree ? 0 : courseViewModel.Price;
                    course.LastUpdated = DateTime.UtcNow;

                    if (courseViewModel.ImageFile != null)
                    {
                        var fileName = Path.GetFileName(courseViewModel.ImageFile.FileName);
                        var uniqueFileName = Path.GetFileNameWithoutExtension(fileName) 
                            + "_" + Guid.NewGuid().ToString().Substring(0, 8) 
                            + Path.GetExtension(fileName);

                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "courses");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await courseViewModel.ImageFile.CopyToAsync(fileStream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(course.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                                course.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        course.ImageUrl = "/images/courses/" + uniqueFileName;
                    }

                    _logger.LogInformation("Calling SaveChanges...");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Changes saved successfully");

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error");
                    if (!CourseExists(courseViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating course");
                    _logger.LogInformation($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", "An error occurred while updating the course. Please try again.");
                }
            }
            else
            {
                _logger.LogInformation("ModelState is invalid:");
                foreach (var modelStateEntry in ModelState)
                {
                    _logger.LogInformation($"Key: {modelStateEntry.Key}");
                    foreach (var error in modelStateEntry.Value.Errors)
                    {
                        _logger.LogInformation($"Error for {modelStateEntry.Key}: {error.ErrorMessage}");
                    }
                }
            }

            return View(courseViewModel);
        }

        // GET: Course/Delete/5
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                // Delete course image if exists
                if (!string.IsNullOrEmpty(course.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", 
                        course.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Course/Enroll/5
        [HttpPost]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> Enroll(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Проверяем существование курса
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Проверяем, не записан ли уже пользователь на курс
            var exists = await _context.UserCourses
                .AnyAsync(uc => uc.UserId == userId && uc.CourseId == id);

            if (!exists)
            {
                // Если курс платный, перенаправляем на страницу подтверждения
                if (!course.IsFree)
                {
                    TempData["PendingCourseId"] = id;
                    return RedirectToAction(nameof(ConfirmEnrollment), new { id });
                }

                var userCourse = new UserCourse
                {
                    UserId = userId,
                    CourseId = id,
                    EnrollmentDate = DateTime.UtcNow,
                    Progress = 0
                };

                try
                {
                    _context.UserCourses.Add(userCourse);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Successfully enrolled in the course!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enrolling in course");
                    TempData["Error"] = "Failed to enroll in the course. Please try again.";
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Course/ConfirmEnrollment/5
        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> ConfirmEnrollment(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            if (course.IsFree)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(course);
        }

        // POST: Course/ConfirmEnrollment/5
        [HttpPost]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> ConfirmEnrollment(int id, bool confirm)
        {
            if (!confirm)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _context.Courses.FindAsync(id);
            
            if (course == null)
            {
                return NotFound();
            }

            var userCourse = new UserCourse
            {
                UserId = userId,
                CourseId = id,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0
            };

            try
            {
                _context.UserCourses.Add(userCourse);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Successfully enrolled in the course!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course");
                TempData["Error"] = "Failed to enroll in the course. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(MyCourses));
        }

        // POST: Course/Unenroll/5
        [HttpPost]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> Unenroll(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userCourse = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == id);

            if (userCourse != null)
            {
                _context.UserCourses.Remove(userCourse);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Course/MyCourses
        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrolledCourses = await _context.UserCourses
                .Include(uc => uc.Course)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            return View(enrolledCourses);
        }

        // GET: Course/Learn/5
        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> Learn(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == id);

            if (enrollment == null)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Progress = enrollment.Progress;
            ViewBag.CurrentContent = null;
            ViewBag.CurrentContentId = null;
            ViewBag.PreviousContentId = null;
            ViewBag.NextContentId = null;

            return View(course);
        }

        // GET: Course/GetContent
        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> GetContent(int contentId)
        {
            var content = await _context.ModuleContents
                .Include(mc => mc.Module)
                    .ThenInclude(m => m.Course)
                .Include(mc => mc.CompletedByUsers)
                .FirstOrDefaultAsync(mc => mc.Id == contentId);

            if (content == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == content.Module.CourseId);

            if (enrollment == null)
            {
                return Unauthorized();
            }

            // Calculate progress
            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Contents)
                    .ThenInclude(c => c.CompletedByUsers)
                .FirstOrDefaultAsync(c => c.Id == content.Module.CourseId);

            var totalContents = course.Modules.Sum(m => m.Contents.Count);
            var completedContents = course.Modules
                .SelectMany(m => m.Contents)
                .Count(c => c.CompletedByUsers.Any(u => u.UserId == userId));

            var progress = (int)((double)completedContents / totalContents * 100);
            ViewBag.Progress = progress;
            ViewBag.IsCompleted = content.CompletedByUsers.Any(u => u.UserId == userId);

            return PartialView("_ContentPartial", content);
        }

        // GET: Course/GetProgress
        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> GetProgress(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (enrollment == null)
            {
                return Json(new { progress = 0 });
            }

            return Json(new { progress = enrollment.Progress });
        }

        // POST: Course/CompleteCourse
        [HttpPost]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> CompleteCourse(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var enrollment = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

            if (enrollment == null)
            {
                return Json(new { success = false });
            }

            enrollment.Progress = 100;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkContentAsCompleted(int contentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false });
            }

            var content = await _context.ModuleContents
                .Include(c => c.CompletedByUsers)
                .FirstOrDefaultAsync(c => c.Id == contentId);

            if (content == null)
            {
                return Json(new { success = false });
            }

            var completion = new CompletedContent
            {
                UserId = userId,
                ContentId = contentId,
                CompletedDate = DateTime.UtcNow
            };

            if (!content.CompletedByUsers.Any(c => c.UserId == userId))
            {
                content.CompletedByUsers.Add(completion);
                await _context.SaveChangesAsync();
            }

            // Calculate progress
            var module = await _context.CourseModules
                .Include(m => m.Contents)
                .FirstOrDefaultAsync(m => m.Contents.Any(c => c.Id == contentId));

            var course = await _context.Courses
                .Include(c => c.Modules)
                .ThenInclude(m => m.Contents)
                .ThenInclude(c => c.CompletedByUsers)
                .FirstOrDefaultAsync(c => c.Modules.Any(m => m.Id == module.Id));

            var totalContents = course.Modules.Sum(m => m.Contents.Count);
            var completedContents = course.Modules
                .SelectMany(m => m.Contents)
                .Count(c => c.CompletedByUsers.Any(u => u.UserId == userId));

            var progress = (int)((double)completedContents / totalContents * 100);

            return Json(new { success = true, progress });
        }

        // GET: api/Course/Search
        [HttpGet]
        [Route("api/Course/Search")]
        public async Task<IActionResult> SearchApi(string query)
        {
            if (string.IsNullOrEmpty(query))
                return Json(new List<Course>());

            var courses = await _context.Courses
                .Where(c => c.Title.Contains(query) ||
                           c.Description.Contains(query) ||
                           c.Author.Contains(query))
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Author,
                    c.Duration,
                    c.Difficulty,
                    c.Price,
                    c.IsFree,
                    c.ImageUrl,
                    EnrolledCount = c.EnrolledUsers.Count
                })
                .Take(10)
                .ToListAsync();

            return Json(courses);
        }

        // GET: Course/Search
        [HttpGet]
        [Route("Course/Search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(await _context.Courses.ToListAsync());

            var courses = await _context.Courses
                .Where(c => c.Title.Contains(query) || 
                           c.Description.Contains(query) ||
                           c.Category.Contains(query))
                .ToListAsync();

            return Json(courses);
        }

        // GET: /Course/Search
        [HttpGet]
        [Route("Course/SearchWithFilters")]
        public async Task<IActionResult> SearchWithFilters(string query = "")
        {
            try
            {
                _logger.LogInformation($"Starting search with query: '{query}'");

                var coursesQuery = _context.Courses
                    .Include(c => c.EnrolledUsers)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    query = query.Trim().ToLower();
                    coursesQuery = coursesQuery.Where(c =>
                        c.Title.ToLower().Contains(query) ||
                        c.Description.ToLower().Contains(query) ||
                        c.Author.ToLower().Contains(query) ||
                        c.Category.ToLower().Contains(query));
                }

                var courses = await coursesQuery
                    .Select(c => new CourseViewModel
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        Author = c.Author,
                        Category = c.Category,
                        Duration = c.Duration,
                        Difficulty = c.Difficulty,
                        IsFree = c.IsFree,
                        Price = c.Price,
                        ImageUrl = c.ImageUrl ?? "",
                        EnrolledCount = c.EnrolledUsers.Count
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {courses.Count} courses");
                return Json(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching courses");
                return StatusCode(500, new { success = false, message = "Произошла ошибка при поиске курсов" });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Purchase(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Имитация процесса оплаты
            // В реальном приложении здесь был бы код для обработки платежа
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Проверяем, не купил ли пользователь уже этот курс
            var existingPurchase = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == id);
                
            if (existingPurchase == null)
            {
                var userCourse = new UserCourse
                {
                    UserId = userId,
                    CourseId = id,
                    PurchaseDate = DateTime.UtcNow
                };
                
                _context.UserCourses.Add(userCourse);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Покупка успешно завершена" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Проверяем, купил ли пользователь курс
            var purchase = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == id);

            if (purchase == null && !course.IsFree)
            {
                return Unauthorized("Вы должны купить этот курс, прежде чем скачать его");
            }

            // Здесь должна быть логика для создания файла курса
            // В данном примере мы создаем простой текстовый файл
            var courseContent = $"Содержание курса: {course.Title}\nАвтор: {course.Author}\n\nОписание:\n{course.Description}";
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(courseContent);
            
            string fileName = $"{course.Title.Replace(" ", "_")}_content.txt";
            
            return File(fileBytes, "application/octet-stream", fileName);
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
