using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cloud_infrastructure.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public string? Message { get; set; }

        public void OnGet(string? message = null)
        {
            Message = message ?? "Your account does not have the required role to view this page.";
        }
    }
}
