using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cloud_infrastructure.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("ServerInstances")]
    public class ServerInstancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServerInstancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServerInstances.ToListAsync());
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var server = await _context.ServerInstances
                .Include(s => s.Developer)
                .Include(s => s.ServerSoftwares)
                    .ThenInclude(ss => ss.SoftwarePackage)
                .FirstOrDefaultAsync(s => s.ServerInstanceId == id);
            if (server == null) return NotFound();
            return View(server);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServerInstance serverInstance)
        {
            if (!ModelState.IsValid)
                return View(serverInstance);

            // Default status when created
            if (string.IsNullOrWhiteSpace(serverInstance.Status))
                serverInstance.Status = "Pending";

            _context.Add(serverInstance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();
            return View(server);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServerInstance serverInstance)
        {
            if (!ModelState.IsValid)
                return View(serverInstance);

            var server = await _context.ServerInstances.FindAsync(serverInstance.ServerInstanceId);
            if (server == null) return NotFound();

            server.Hostname = serverInstance.Hostname;
            server.RamGb = serverInstance.RamGb;
            server.InstanceSize = serverInstance.InstanceSize;
            server.Status = serverInstance.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();

            _context.ServerInstances.Remove(server);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();

            server.Status = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var server = await _context.ServerInstances.FindAsync(id);
            if (server == null) return NotFound();

            server.Status = "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
