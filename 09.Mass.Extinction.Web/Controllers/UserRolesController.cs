﻿namespace Ninth.Mass.Extinction.Web.Controllers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extinction.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ViewModels.RoleManager;
using ViewModels.UserRoles;

[Authorize(Roles = "Admin")]
public class UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users.ToListAsync();
        var userRolesViewModel = new List<UserRolesViewModel>();
        foreach (var user in users)
        {
            var thisViewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = await GetUserRoles(user)
            };
            userRolesViewModel.Add(thisViewModel);
        }

        return View(userRolesViewModel);
    }

    public async Task<IActionResult> Manage(string userId)
    {
        ViewBag.userId = userId;
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
            // ReSharper disable once Mvc.ViewNotResolved
            return View("NotFound");
        }

        ViewBag.UserName = user.UserName;
        var model = new List<ManageUserRolesViewModel>();
        var roles = await roleManager.Roles.ToListAsync();
        var userRoles = await GetUserRoles(user);

        foreach (var role in roles)
        {
            var userRolesViewModel = new ManageUserRolesViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Selected = userRoles.Contains(role.Name)
            };

            model.Add(userRolesViewModel);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return View();
        }

        var roles = await userManager.GetRolesAsync(user);
        var result = await userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot remove user existing roles");
            return View(model);
        }

        result = await userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
        if (result.Succeeded)
        {
            return RedirectToAction("Index");
        }

        ModelState.AddModelError("", "Cannot add selected roles to user");
        return View(model);
    }

    private async Task<List<string>> GetUserRoles(ApplicationUser user) => new(await userManager.GetRolesAsync(user));
}
