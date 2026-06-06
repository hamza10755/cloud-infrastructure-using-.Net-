using cloud_infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace cloud_infrastructure.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet("")]
        [HttpGet("Home")]
        [HttpGet("Home/Index")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("Developer"))
                {
                    return RedirectToAction("RequestVM", "Developer");
                }
            }

            ViewData["PortalMessage"] = "Welcome to the Self-Service Cloud Infrastructure Provisioning Portal.";
            ViewBag.ShowInfoAlert = true;
            ViewBag.MainHeader = "Cloud Provisioning Dashboard";

            return View();
        }

        [HttpGet("Home/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
