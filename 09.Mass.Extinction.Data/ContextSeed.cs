namespace _09.Mass.Extinction.Data;

using System.Linq;
using System.Threading.Tasks;
using Entities;
using Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

public class ContextSeed
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        //Seed Roles
        if (await roleManager.FindByNameAsync(Roles.DiscordAdmin.ToString()) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.DiscordAdmin.ToString()));
        }

        if (await roleManager.FindByNameAsync(Roles.Admin.ToString()) == null)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
        }
    }

    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        var adminUser = new ApplicationUser {
            UserName = configuration.GetValue<string>("Application:AdminEmail"),
            Name = "Admin User",
            ProfilePicture = "https://cdn.discordapp.com/avatars/850452578705735720/ecd4aacda4e908f90fc15c01dc196a14.png",
            Email = configuration.GetValue<string>("Application:AdminEmail"),
            EmailConfirmed = true
        };
        if (userManager.Users.All(u => u.Id != adminUser.Id))
        {
            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(adminUser, configuration.GetValue<string>("Application:AdminPassword"));
                await userManager.AddToRoleAsync(adminUser, Roles.DiscordAdmin.ToString());
                await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
            }
        }
    }
}
