using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EduSite.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EduSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(user);
            }
            return View(users);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,FirstName,LastName")] ApplicationUser user, string[] selectedRoles)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userToUpdate = await _userManager.FindByIdAsync(id);
                if (userToUpdate == null)
                {
                    return NotFound();
                }

                userToUpdate.Email = user.Email;
                userToUpdate.FirstName = user.FirstName;
                userToUpdate.LastName = user.LastName;

                var result = await _userManager.UpdateAsync(userToUpdate);
                if (result.Succeeded)
                {
                    // Update roles
                    var userRoles = await _userManager.GetRolesAsync(userToUpdate);
                    selectedRoles = selectedRoles ?? new string[] { };

                    var rolesToAdd = selectedRoles.Except(userRoles);
                    var rolesToRemove = userRoles.Except(selectedRoles);

                    await _userManager.AddToRolesAsync(userToUpdate, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(userToUpdate, rolesToRemove);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var currentUserRoles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            ViewBag.UserRoles = currentUserRoles;

            return View(user);
        }

        // GET: UserManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(user);
        }
    }
}
