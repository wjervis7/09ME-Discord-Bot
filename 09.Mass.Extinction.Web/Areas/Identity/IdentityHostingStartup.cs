using Microsoft.AspNetCore.Hosting;
using Ninth.Mass.Extinction.Web.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]

namespace Ninth.Mass.Extinction.Web.Areas.Identity;

using Microsoft.AspNetCore.Hosting;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) =>
        builder.ConfigureServices((_, _) =>
        {
        });
}
