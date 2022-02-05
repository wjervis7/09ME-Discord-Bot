using _09.Mass.Extinction.Web.Areas.Identity;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace _09.Mass.Extinction.Web.Areas.Identity;

using Microsoft.AspNetCore.Hosting;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) =>
        builder.ConfigureServices((_, _) =>
        {
        });
}
