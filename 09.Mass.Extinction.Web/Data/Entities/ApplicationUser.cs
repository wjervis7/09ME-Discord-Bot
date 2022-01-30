namespace _09.Mass.Extinction.Web.Data.Entities;

using Microsoft.AspNetCore.Identity;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData] public string Name { get; set; }

    [PersonalData] public string ProfilePicture { get; set; }
}
