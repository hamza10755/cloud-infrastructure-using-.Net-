using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cloud_infrastructure.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalServers = await _context.ServerInstances.CountAsync();

            ViewBag.TotalPackages = await _context.SoftwarePackages.CountAsync();

            ViewBag.PendingServers = await _context.ServerInstances
                .Where(s => s.Status == "Pending")
                .CountAsync();

            ViewBag.ApprovedServers = await _context.ServerInstances
                .Where(s => s.Status == "Approved")
                .CountAsync();

            ViewBag.RejectedServers = await _context.ServerInstances
                .Where(s => s.Status == "Rejected")
                .CountAsync();

            return View();
        }
    }
}
