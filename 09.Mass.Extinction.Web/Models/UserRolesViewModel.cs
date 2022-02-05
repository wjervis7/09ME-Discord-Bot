namespace _09.Mass.Extinction.Web.Models;

using System.Collections.Generic;

public class UserRolesViewModel
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public IEnumerable<string> Roles { get; set; }
}
