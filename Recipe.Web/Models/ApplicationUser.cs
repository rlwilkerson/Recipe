using Microsoft.AspNetCore.Identity;

namespace Recipe.Web.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<Cookbook> Cookbooks { get; set; } = [];
    public ICollection<Models.Recipe> Recipes { get; set; } = [];
}
