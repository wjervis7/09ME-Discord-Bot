namespace Ninth.Mass.Extinction.Web.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class RoleManagerController(RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles.ToListAsync();
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> AddRole(string roleName)
    {
        if (roleName != null)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        }

        return RedirectToAction("Index");
    }
}
