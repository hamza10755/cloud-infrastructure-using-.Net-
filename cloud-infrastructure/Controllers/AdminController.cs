using cloud_infrastructure.Models;
using cloud_infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cloud_infrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Developer> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<Developer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pendingRequests = await _context.ServerInstances
                .Include(server => server.Developer)
                .Where(server => server.Status == "Pending")
                .OrderByDescending(server => server.ServerInstanceId)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalServers = await _context.ServerInstances.CountAsync(),
                TotalPackages = await _context.SoftwarePackages.CountAsync(),
                PendingServers = pendingRequests.Count,
                ApprovedServers = await _context.ServerInstances.CountAsync(server => server.Status == "Approved"),
                RejectedServers = await _context.ServerInstances.CountAsync(server => server.Status == "Rejected"),
                PendingRequests = pendingRequests
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveServer(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null)
            {
                return NotFound();
            }

            server.Status = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectServer(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null)
            {
                return NotFound();
            }

            server.Status = "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            var model = new ManageUsersViewModel();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Users.Add(new UserWithRoles
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    Roles = roles.ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to promote user to Admin.");
                return RedirectToAction(nameof(ManageUsers));
            }

            TempData["SuccessMessage"] = $"Promoted {user.Email} to Admin.";
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}
