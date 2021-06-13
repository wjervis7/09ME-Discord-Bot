namespace _09.Mass.Extinction.Web.Data
{
    using System.Linq;
    using System.Threading.Tasks;
    using Enums;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;

    public class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
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

        public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            var adminUser = new IdentityUser
            {
                UserName = configuration.GetValue<string>("Application:AdminEmail"),
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
}
