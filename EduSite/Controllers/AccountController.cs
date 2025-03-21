using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EduSite.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System;
using EduSite.Data;

namespace EduSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateJoined = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Include enrolled courses
            var userWithCourses = await _context.Users
                .Include(u => u.EnrolledCourses)
                    .ThenInclude(uc => uc.Course)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            return View(userWithCourses);
        }

        // POST: /Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ApplicationUser model, IFormFile ProfilePicture)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Invalid model state" });
                }
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "User not found" });
                }
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (ProfilePicture != null)
            {
                // Handle profile picture upload
                var fileName = Path.GetFileName(ProfilePicture.FileName);
                var uniqueFileName = Path.GetFileNameWithoutExtension(fileName) 
                    + "_" + Guid.NewGuid().ToString().Substring(0, 8) 
                    + Path.GetExtension(fileName);

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = "/uploads/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Обновляем сессию
                HttpContext.Session.SetString("FirstName", user.FirstName);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Failed to update profile" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Profile", model);
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation password do not match.";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            TempData["Error"] = "Failed to change password. Please check your current password.";
            return RedirectToAction(nameof(Profile));
        }
    }
}
