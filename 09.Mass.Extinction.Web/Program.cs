using System;
using _09.Mass.Extinction.Data;
using _09.Mass.Extinction.Data.Entities;
using _09.Mass.Extinction.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureHost(builder.Host);
Startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

Startup.ConfigureApp(app, builder.Environment);

await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        await ContextSeed.SeedRolesAsync(roleManager);
        await ContextSeed.SeedAdminUserAsync(userManager, configuration);

        await context.SaveChangesAsync();
    }
    catch (Exception e)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(e, "An error occurred while seeding the DB.");
    }
}

await app.RunAsync();
