using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace cloud_infrastructure.Controllers
{
    [Authorize]
    [Route("Infrastructure")]
    public class InfrastructureController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InfrastructureController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var pending = await _context.ServerInstances
                .Where(s => s.Status == "Pending")
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/Home/Index.cshtml", pending);
        }

        [HttpPost("RequestServer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestServer(ServerInstance newServer)
        {
            if (!ModelState.IsValid)
            {
                // If model is invalid, send user back to home with validation messages
                return RedirectToAction("Index", "ServerInstances");
            }

            newServer.Status = "Pending";
            newServer.DeveloperId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.ServerInstances.Add(newServer);
            await _context.SaveChangesAsync();

            // Redirect to the ServerInstances list so the requester can see the created request
            return RedirectToAction("RequestVM", "Developer");
        }

        // Approve a pending server
        [HttpPost("ApproveServer")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveServer(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();

            server.Status = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Reject a pending server
        [HttpPost("RejectServer")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectServer(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();

            server.Status = "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}