namespace Ninth.Mass.Extinction.Web.Areas.Identity.Pages.Account;

using System.Text;
using System.Threading.Tasks;
using Extinction.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

[AllowAnonymous]
public class ConfirmEmailModel(UserManager<ApplicationUser> userManager) : PageModel
{
    [TempData]
    public string StatusMessage { get; set; }

    // ReSharper disable once UnusedMember.Global
    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, code);
        StatusMessage = result.Succeeded ? "Thank you for confirming your email. You may now log in." : "Error confirming your email.";
        TempData.Keep(nameof(StatusMessage));
        return Page();
    }
}
