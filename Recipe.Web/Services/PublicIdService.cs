using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Web.Services;

public interface IPublicIdService
{
    Task<string> GenerateForCookbookAsync(AppDbContext db);
    Task<string> GenerateForRecipeAsync(AppDbContext db);
}

public class PublicIdService : IPublicIdService
{
    private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int IdLength = 10;
    private readonly Random _random = new();

    public async Task<string> GenerateForCookbookAsync(AppDbContext db)
    {
        return await GenerateUniqueIdAsync(db, async (ctx, id) => 
            await ctx.Cookbooks.AnyAsync(c => c.PublicId == id));
    }

    public async Task<string> GenerateForRecipeAsync(AppDbContext db)
    {
        return await GenerateUniqueIdAsync(db, async (ctx, id) => 
            await ctx.Recipes.AnyAsync(r => r.PublicId == id));
    }

    private async Task<string> GenerateUniqueIdAsync(AppDbContext db, Func<AppDbContext, string, Task<bool>> existsCheck)
    {
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var id = GenerateRandomBase62String();
            if (!await existsCheck(db, id))
            {
                return id;
            }
        }
        throw new InvalidOperationException("Failed to generate unique PublicId after maximum attempts");
    }

    private string GenerateRandomBase62String()
    {
        var chars = new char[IdLength];
        for (int i = 0; i < IdLength; i++)
        {
            chars[i] = Base62Chars[_random.Next(Base62Chars.Length)];
        }
        return new string(chars);
    }
}
