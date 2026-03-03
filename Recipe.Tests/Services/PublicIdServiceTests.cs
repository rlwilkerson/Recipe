using Microsoft.EntityFrameworkCore;
using Recipe.Tests.TestHelpers;
using Recipe.Web.Models;
using Recipe.Web.Services;

namespace Recipe.Tests.Services;

public class PublicIdServiceTests
{
    private readonly PublicIdService _publicIdService = new();

    [Fact]
    public async Task GenerateForCookbookAsync_ReturnsIdWithCorrectLength()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var publicId = await _publicIdService.GenerateForCookbookAsync(db);
        
        Assert.Equal(10, publicId.Length);
    }

    [Fact]
    public async Task GenerateForCookbookAsync_ContainsOnlyBase62Characters()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var publicId = await _publicIdService.GenerateForCookbookAsync(db);
        
        Assert.Matches("^[0-9a-zA-Z]+$", publicId);
    }

    [Fact]
    public async Task GenerateForCookbookAsync_ReturnsUniqueValues()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var id1 = await _publicIdService.GenerateForCookbookAsync(db);
        var id2 = await _publicIdService.GenerateForCookbookAsync(db);
        
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task GenerateForCookbookAsync_AvoidsCollisions()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        // Add a cookbook with a known PublicId
        var existingCookbook = new Cookbook
        {
            Name = "Test",
            PublicId = "existingId",
            Slug = "test",
            OwnerId = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Cookbooks.Add(existingCookbook);
        await db.SaveChangesAsync();
        
        var newId = await _publicIdService.GenerateForCookbookAsync(db);
        
        Assert.NotEqual("existingId", newId);
    }

    [Fact]
    public async Task GenerateForRecipeAsync_ReturnsIdWithCorrectLength()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var publicId = await _publicIdService.GenerateForRecipeAsync(db);
        
        Assert.Equal(10, publicId.Length);
    }

    [Fact]
    public async Task GenerateForRecipeAsync_ContainsOnlyBase62Characters()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var publicId = await _publicIdService.GenerateForRecipeAsync(db);
        
        Assert.Matches("^[0-9a-zA-Z]+$", publicId);
    }

    [Fact]
    public async Task GenerateForRecipeAsync_ReturnsUniqueValues()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        var id1 = await _publicIdService.GenerateForRecipeAsync(db);
        var id2 = await _publicIdService.GenerateForRecipeAsync(db);
        
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task GenerateForRecipeAsync_AvoidsCollisions()
    {
        var db = DbContextHelper.CreateInMemoryDbContext();
        
        // Add a recipe with a known PublicId
        var existingRecipe = new Web.Models.Recipe
        {
            Title = "Test",
            PublicId = "recipeExId",
            Slug = "test",
            OwnerId = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Recipes.Add(existingRecipe);
        await db.SaveChangesAsync();
        
        var newId = await _publicIdService.GenerateForRecipeAsync(db);
        
        Assert.NotEqual("recipeExId", newId);
    }
}
