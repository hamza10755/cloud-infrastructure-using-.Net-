using cloud_infrastructure.Data;
using cloud_infrastructure.Models;
using cloud_infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace cloud_infrastructure.Controllers
{
    [Authorize(Roles = "Developer,Admin")]
    public class DeveloperController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeveloperController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> RequestVM()
        {
            var model = new DeveloperRequestViewModel
            {
                RecentRequests = await GetRecentRequestsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestVM(DeveloperRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RecentRequests = await GetRecentRequestsAsync();
                return View(model);
            }

            var server = new ServerInstance
            {
                Hostname = model.Hostname,
                RamGb = model.RamGb,
                InstanceSize = model.InstanceSize,
                Purpose = model.Purpose,
                Status = "Pending",
                DeveloperId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.ServerInstances.Add(server);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"VM request '{model.Hostname}' submitted successfully.";
            return RedirectToAction(nameof(RequestVM));
        }

        private async Task<List<ServerInstance>> GetRecentRequestsAsync()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.ServerInstances
                .Include(server => server.Developer)
                .Where(server => server.DeveloperId == currentUserId)
                .OrderByDescending(server => server.ServerInstanceId)
                .Take(10)
                .ToListAsync();
        }
    }
}