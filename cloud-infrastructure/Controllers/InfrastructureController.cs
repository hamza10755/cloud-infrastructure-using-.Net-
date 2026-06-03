using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cloud_infrastructure.Controllers
{
    public class InfrastructureController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InfrastructureController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult RequestServer(ServerInstance newServer)
        {
            newServer.Status = "Pending";
            newServer.DeveloperId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.ServerInstances.Add(newServer);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}