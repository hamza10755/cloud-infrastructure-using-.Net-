using cloud_infrastructure.Models;
using cloud_infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cloud_infrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Developer> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<Developer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
        [HttpGet("Dashboard")]
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

        [HttpPost("ApproveServer")]
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

        [HttpPost("RejectServer")]
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

        [HttpGet("ManageUsers")]
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
                        UserName = user.UserName ?? "",
                        Email = user.Email ?? "",
                        Roles = roles.ToList()
                    });
            }

            return View(model);
        }

        [HttpGet("ServerHistory")]
        public async Task<IActionResult> ServerHistory(string? search = null, string? sortOrder = null)
        {
            var query = _context.ServerInstances
                .Include(s => s.Developer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(s =>
                    s.Hostname.ToLower().Contains(term) ||
                    s.OperatingSystem.ToLower().Contains(term));
            }

            // Set up sorting params in ViewBag
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = string.IsNullOrEmpty(sortOrder) ? "id_asc" : "";
            ViewBag.HostnameSortParm = sortOrder == "hostname_asc" ? "hostname_desc" : "hostname_asc";
            ViewBag.RamSortParm = sortOrder == "ram_asc" ? "ram_desc" : "ram_asc";
            ViewBag.OsSortParm = sortOrder == "os_asc" ? "os_desc" : "os_asc";
            ViewBag.StatusSortParm = sortOrder == "status_asc" ? "status_desc" : "status_asc";

            query = sortOrder switch
            {
                "id_asc" => query.OrderBy(s => s.ServerInstanceId),
                "hostname_asc" => query.OrderBy(s => s.Hostname),
                "hostname_desc" => query.OrderByDescending(s => s.Hostname),
                "ram_asc" => query.OrderBy(s => s.RamGb),
                "ram_desc" => query.OrderByDescending(s => s.RamGb),
                "os_asc" => query.OrderBy(s => s.OperatingSystem),
                "os_desc" => query.OrderByDescending(s => s.OperatingSystem),
                "status_asc" => query.OrderBy(s => s.Status),
                "status_desc" => query.OrderByDescending(s => s.Status),
                _ => query.OrderByDescending(s => s.ServerInstanceId) // Default: Newest first (id_desc)
            };

            var model = new ServerHistoryViewModel
            {
                Search = search,
                SortOrder = sortOrder,
                Servers = await query.ToListAsync()
            };

            return View(model);
        }

        [HttpGet("ServerDetails/{id}")]
        public async Task<IActionResult> ServerDetails(int id)
        {
            var server = await _context.ServerInstances
                .Include(s => s.Developer)
                .Include(s => s.ServerSoftwares)
                    .ThenInclude(ss => ss.SoftwarePackage)
                .FirstOrDefaultAsync(s => s.ServerInstanceId == id);

            if (server == null)
            {
                return NotFound();
            }

            return View(server);
        }

        [HttpPost("UpdateRoles")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (string.Equals(role, "Developer", StringComparison.OrdinalIgnoreCase))
            {
                if (currentRoles.Contains("Admin"))
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!currentRoles.Contains("Developer"))
                    await _userManager.AddToRoleAsync(user, "Developer");
            }
            else if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (currentRoles.Contains("Developer"))
                    await _userManager.RemoveFromRoleAsync(user, "Developer");
                if (!currentRoles.Contains("Admin"))
                    await _userManager.AddToRoleAsync(user, "Admin");
            }
            else if (string.Equals(role, "Both", StringComparison.OrdinalIgnoreCase))
            {
                if (!currentRoles.Contains("Admin"))
                    await _userManager.AddToRoleAsync(user, "Admin");
                if (!currentRoles.Contains("Developer"))
                    await _userManager.AddToRoleAsync(user, "Developer");
            }

            TempData["SuccessMessage"] = $"Updated roles for {user.UserName}.";
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}
