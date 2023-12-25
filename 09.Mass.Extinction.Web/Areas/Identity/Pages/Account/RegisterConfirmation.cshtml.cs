namespace Ninth.Mass.Extinction.Web.Areas.Identity.Pages.Account;

using System.Text;
using System.Threading.Tasks;
using Extinction.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

[AllowAnonymous]
public class RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender) : PageModel
{
    public string Email { get; set; }

    public bool DisplayConfirmAccountLink { get; set; }

    public string EmailConfirmationUrl { get; set; }

    // ReSharper disable once UnusedMember.Global
    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (email == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        Email = email;

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        EmailConfirmationUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new
            {
                area = "Identity",
                userId,
                code,
                returnUrl
            },
            Request.Scheme);

        return Page();
    }
}
