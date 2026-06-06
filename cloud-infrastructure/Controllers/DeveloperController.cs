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
    [Route("Developer")]
    public class DeveloperController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeveloperController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("RequestVM/{id?}")]
        public async Task<IActionResult> RequestVM(int? id = null)
        {
            var model = new DeveloperRequestViewModel
            {
                RecentRequests = await GetRecentRequestsAsync(),
                AvailableSoftware = await _context.SoftwarePackages.ToListAsync()
            };

            if (id.HasValue)
            {
                var server = await _context.ServerInstances
                    .Include(s => s.ServerSoftwares)
                    .FirstOrDefaultAsync(s => s.ServerInstanceId == id && s.DeveloperId == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (server == null || server.Status != "Pending")
                {
                    return NotFound();
                }

                model.EditId = server.ServerInstanceId;
                model.Hostname = server.Hostname;
                model.RamGb = server.RamGb;
                model.InstanceSize = server.InstanceSize;
                model.OperatingSystem = server.OperatingSystem;
                model.Purpose = server.Purpose;
                model.SelectedSoftwareIds = server.ServerSoftwares.Select(ss => ss.SoftwarePackageId).ToList();
            }

            return View(model);
        }

        [HttpPost("RequestVM/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestVM(DeveloperRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RecentRequests = await GetRecentRequestsAsync();
                model.AvailableSoftware = await _context.SoftwarePackages.ToListAsync();
                return View(model);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model.EditId.HasValue)
            {
                var server = await _context.ServerInstances
                    .Include(s => s.ServerSoftwares)
                    .FirstOrDefaultAsync(s => s.ServerInstanceId == model.EditId && s.DeveloperId == currentUserId);

                if (server == null || server.Status != "Pending")
                {
                    return NotFound();
                }

                server.Hostname = model.Hostname;
                server.RamGb = model.RamGb;
                server.InstanceSize = model.InstanceSize;
                server.OperatingSystem = model.OperatingSystem;
                server.Purpose = model.Purpose;

                // Update junction table
                server.ServerSoftwares.Clear();
                if (model.SelectedSoftwareIds != null)
                {
                    foreach (var packageId in model.SelectedSoftwareIds)
                    {
                        server.ServerSoftwares.Add(new ServerSoftware { ServerInstanceId = server.ServerInstanceId, SoftwarePackageId = packageId });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"VM request '{model.Hostname}' updated successfully.";
                return RedirectToAction(nameof(RequestVM), new { id = (int?)null });
            }

            var newServer = new ServerInstance
            {
                Hostname = model.Hostname,
                RamGb = model.RamGb,
                InstanceSize = model.InstanceSize,
                OperatingSystem = model.OperatingSystem,
                Purpose = model.Purpose,
                Status = "Pending",
                DeveloperId = currentUserId
            };

            if (model.SelectedSoftwareIds != null)
            {
                foreach (var packageId in model.SelectedSoftwareIds)
                {
                    newServer.ServerSoftwares.Add(new ServerSoftware { SoftwarePackageId = packageId });
                }
            }

            _context.ServerInstances.Add(newServer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"VM request '{model.Hostname}' submitted successfully.";
            return RedirectToAction(nameof(RequestVM), new { id = (int?)null });
        }

        [HttpPost("DeleteRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var server = await _context.ServerInstances
                .FirstOrDefaultAsync(s => s.ServerInstanceId == id && s.DeveloperId == currentUserId);

            if (server == null || server.Status != "Pending")
            {
                return NotFound();
            }

            _context.ServerInstances.Remove(server);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"VM request '{server.Hostname}' deleted.";
            return RedirectToAction(nameof(RequestVM), new { id = (int?)null });
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