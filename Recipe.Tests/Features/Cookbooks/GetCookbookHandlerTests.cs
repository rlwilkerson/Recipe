using Microsoft.EntityFrameworkCore;
using Recipe.Tests.TestHelpers;
using Recipe.Web.Features.Cookbooks.GetCookbook;
using Recipe.Web.Models;

namespace Recipe.Tests.Features.Cookbooks;

public class GetCookbookHandlerTests
{
    [Fact]
    public async Task Handle_OwnerCanReadOwnCookbook()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = "owner1", UserName = "testuser" };
        
        var cookbook = new Cookbook
        {
            Name = "My Cookbook",
            PublicId = "pub123",
            Slug = "my-cookbook",
            OwnerId = "owner1",
            Owner = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Cookbooks.Add(cookbook);
        await db.SaveChangesAsync();
        
        var handler = new GetCookbookHandler(db);
        var query = new GetCookbookQuery("pub123", "owner1");
        
        // Act
        var response = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal("pub123", response.PublicId);
        Assert.Equal("my-cookbook", response.Slug);
        Assert.Equal("My Cookbook", response.Name);
        Assert.Equal("testuser", response.OwnerName);
    }

    [Fact]
    public async Task Handle_ReturnsNullForUnknownPublicId()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetCookbookHandler(db);
        var query = new GetCookbookQuery("nonexistent", null);
        
        // Act
        var response = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task Handle_IncludesRecipesInCookbook()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = "owner1", UserName = "testuser" };
        
        var cookbook = new Cookbook
        {
            Name = "My Cookbook",
            PublicId = "pub123",
            Slug = "my-cookbook",
            OwnerId = "owner1",
            Owner = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var recipe1 = new Web.Models.Recipe
        {
            Title = "Recipe 1",
            PublicId = "rec1",
            Slug = "recipe-1",
            OwnerId = "owner1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var recipe2 = new Web.Models.Recipe
        {
            Title = "Recipe 2",
            PublicId = "rec2",
            Slug = "recipe-2",
            OwnerId = "owner1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Cookbooks.Add(cookbook);
        db.Recipes.AddRange(recipe1, recipe2);
        await db.SaveChangesAsync();
        
        var cookbookRecipe1 = new CookbookRecipe
        {
            CookbookId = cookbook.Id,
            RecipeId = recipe1.Id,
            SortOrder = 1
        };
        
        var cookbookRecipe2 = new CookbookRecipe
        {
            CookbookId = cookbook.Id,
            RecipeId = recipe2.Id,
            SortOrder = 0
        };
        
        db.CookbookRecipes.AddRange(cookbookRecipe1, cookbookRecipe2);
        await db.SaveChangesAsync();
        
        var handler = new GetCookbookHandler(db);
        var query = new GetCookbookQuery("pub123", "owner1");
        
        // Act
        var response = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Recipes.Count);
        // Should be sorted by SortOrder
        Assert.Equal("rec2", response.Recipes[0].PublicId);
        Assert.Equal("Recipe 2", response.Recipes[0].Title);
        Assert.Equal(0, response.Recipes[0].SortOrder);
        Assert.Equal("rec1", response.Recipes[1].PublicId);
        Assert.Equal("Recipe 1", response.Recipes[1].Title);
        Assert.Equal(1, response.Recipes[1].SortOrder);
    }

    [Fact]
    public async Task Handle_IncludesDescriptionWhenPresent()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = "owner1", UserName = "testuser" };
        
        var cookbook = new Cookbook
        {
            Name = "My Cookbook",
            Description = "A wonderful collection",
            PublicId = "pub123",
            Slug = "my-cookbook",
            OwnerId = "owner1",
            Owner = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Cookbooks.Add(cookbook);
        await db.SaveChangesAsync();
        
        var handler = new GetCookbookHandler(db);
        var query = new GetCookbookQuery("pub123", "owner1");
        
        // Act
        var response = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal("A wonderful collection", response.Description);
    }

    [Fact]
    public async Task Handle_ReturnsCreatedAtTimestamp()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var user = new ApplicationUser { Id = "owner1", UserName = "testuser" };
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        
        var cookbook = new Cookbook
        {
            Name = "My Cookbook",
            PublicId = "pub123",
            Slug = "my-cookbook",
            OwnerId = "owner1",
            Owner = user,
            CreatedAt = createdAt,
            UpdatedAt = DateTime.UtcNow
        };
        db.Cookbooks.Add(cookbook);
        await db.SaveChangesAsync();
        
        var handler = new GetCookbookHandler(db);
        var query = new GetCookbookQuery("pub123", "owner1");
        
        // Act
        var response = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(createdAt, response.CreatedAt);
    }
}
