namespace Ninth.Mass.Extinction.Web;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Discord;
using Email;
using Extinction.Data;
using Extinction.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder();
        var dbHost = configuration.GetValue<string>("Database:Host");
        var dbPort = configuration.GetValue<int>("Database:Port");
        var dbDatabase = configuration.GetValue<string>("Database:Database");
        var dbUser = configuration.GetValue<string>("Database:User");
        var dbPassword = configuration.GetValue<string>("Database:Password");

        connectionStringBuilder["Server"] = $"{dbHost},{dbPort}";
        connectionStringBuilder["Database"] = dbDatabase;
        connectionStringBuilder.UserID = dbUser;
        connectionStringBuilder.Password = dbPassword;
        connectionStringBuilder.TrustServerCertificate = true;

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionStringBuilder.ToString()));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddTransient<IEmailSender, ZohoEmailSender>();
        services.Configure<AuthMessageSenderOptions>(configuration.GetSection("Email"));

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 2;
            options.Password.RequiredLength = 12;
        });

        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddAuthentication()
            .AddDiscord(options =>
            {
                var discordAuthSection = configuration.GetSection("Discord");
                options.ClientId = discordAuthSection["ClientId"] ?? throw new InvalidOperationException("Discord:ClientId cannot be null.");
                options.ClientSecret = discordAuthSection["ClientSecret"] ?? throw new InvalidOperationException("Discord:ClientSecret cannot be null.");
                options.Scope.Add("email");

                options.SaveTokens = true;
            });

        services.Configure<DiscordConfiguration>(configuration.GetSection("Discord"));
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IOptions<DiscordConfiguration>>().Value;
            var client = new HttpClient
            {
                BaseAddress = new Uri(config.ApiEndpoint),
                DefaultRequestHeaders =
                {
                    Authorization = AuthenticationHeaderValue.Parse($"Bot {config.Token}")
                }
            };
            return client;
        });
        services.AddSingleton<DiscordService>();

        services.AddControllersWithViews();
    }

    public static void ConfigureApp(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }

    public static void ConfigureHost(IHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, configuration) =>
        {
            var isDocker = context.Configuration.GetValue<bool?>("IsDocker");
            if (isDocker != true)
            {
                return;
            }

            configuration.AddJsonFile("/configs/appsettings.web.json", true, true);
            configuration.AddKeyPerFile("/run/secrets", true, true);
        });

        host.UseSerilog((hostingContext, _, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));
    }
}
