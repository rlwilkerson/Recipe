using System.Text;
using System.Text.RegularExpressions;

namespace Recipe.Web.Services;

public interface ISlugService
{
    string GenerateSlug(string input);
}

public class SlugService : ISlugService
{
    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Lowercase
        var slug = input.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove non-alphanumeric-hyphen characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Collapse multiple hyphens to single hyphen
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim hyphens from ends
        slug = slug.Trim('-');

        return slug;
    }
}
