using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ITO_TicketManagementSystem.Models;

namespace ITO_TicketManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // POST: Assign role to a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string email, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Email and role are required.";
                return RedirectToAction("Dashboard", "Home");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = $"User with email {email} not found.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                TempData["Error"] = $"Role '{role}' does not exist.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["Success"] = $"Assigned role '{role}' to {email}.";
            }
            else
            {
                TempData["Info"] = $"{email} already has role '{role}'.";
            }

            return RedirectToAction("Dashboard", "Home");
        }

        // POST: Remove role from a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string email, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Email and role are required.";
                return RedirectToAction("Dashboard", "Home");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = $"User with email {email} not found.";
                return RedirectToAction("Dashboard", "Home");
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.RemoveFromRoleAsync(user, role);
                TempData["Success"] = $"Removed role '{role}' from {email}.";
            }
            else
            {
                TempData["Info"] = $"{email} does not have role '{role}'.";
            }

            return RedirectToAction("Dashboard", "Home");
        }
    }
}
