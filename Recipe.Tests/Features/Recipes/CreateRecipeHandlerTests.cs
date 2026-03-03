using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Recipe.Tests.TestHelpers;
using Recipe.Web.Features.Recipes.CreateRecipe;
using Recipe.Web.Services;

namespace Recipe.Tests.Features.Recipes;

public class CreateRecipeHandlerTests
{
    [Fact]
    public async Task Handle_CreatesRecipeWithAllFields()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("recPubId");
        slugService.GenerateSlug("My Recipe").Returns("my-recipe");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand(
            "My Recipe",
            "Description",
            "Ingredients list",
            "Instructions here",
            30,
            60,
            4,
            "user123");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal("recPubId", response.PublicId);
        Assert.Equal("my-recipe", response.Slug);
        Assert.Equal("My Recipe", response.Title);
    }

    [Fact]
    public async Task Handle_SavesRecipeToDatabase()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("rec123");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("test-recipe");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand(
            "Test Recipe",
            "A test",
            "1 cup flour",
            "Mix and bake",
            15,
            30,
            2,
            "owner1");
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedRecipe = await db.Recipes.FirstOrDefaultAsync(r => r.PublicId == "rec123");
        Assert.NotNull(savedRecipe);
        Assert.Equal("Test Recipe", savedRecipe.Title);
        Assert.Equal("A test", savedRecipe.Description);
        Assert.Equal("1 cup flour", savedRecipe.Ingredients);
        Assert.Equal("Mix and bake", savedRecipe.Instructions);
        Assert.Equal(15, savedRecipe.PrepTime);
        Assert.Equal(30, savedRecipe.CookTime);
        Assert.Equal(2, savedRecipe.Servings);
        Assert.Equal("owner1", savedRecipe.OwnerId);
        Assert.Equal("test-recipe", savedRecipe.Slug);
    }

    [Fact]
    public async Task Handle_UsesPublicIdServiceToGenerateId()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("uniqueRecId");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("Test", null, null, null, null, null, null, "user1");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await publicIdService.Received(1).GenerateForRecipeAsync(db);
        Assert.Equal("uniqueRecId", response.PublicId);
    }

    [Fact]
    public async Task Handle_UsesSlugServiceToGenerateSlug()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("id");
        slugService.GenerateSlug("Grandma's Pie").Returns("grandmas-pie");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("Grandma's Pie", null, null, null, null, null, null, "user1");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        slugService.Received(1).GenerateSlug("Grandma's Pie");
        Assert.Equal("grandmas-pie", response.Slug);
    }

    [Fact]
    public async Task Handle_OriginalRecipeIdIsNullOnCreate()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("newRec");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("New Recipe", null, null, null, null, null, null, "user1");
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedRecipe = await db.Recipes.FirstOrDefaultAsync(r => r.PublicId == "newRec");
        Assert.NotNull(savedRecipe);
        Assert.Null(savedRecipe.OriginalRecipeId);
    }

    [Fact]
    public async Task Handle_SetsCreatedAtTimestamp()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("recId");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("Test", null, null, null, null, null, null, "user1");
        
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        var afterCreate = DateTime.UtcNow;
        
        // Assert
        var savedRecipe = await db.Recipes.FirstOrDefaultAsync(r => r.PublicId == "recId");
        Assert.NotNull(savedRecipe);
        Assert.InRange(savedRecipe.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("recId2");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("Test", null, null, null, null, null, null, "user1");
        
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        var afterCreate = DateTime.UtcNow;
        
        // Assert
        var savedRecipe = await db.Recipes.FirstOrDefaultAsync(r => r.PublicId == "recId2");
        Assert.NotNull(savedRecipe);
        Assert.InRange(savedRecipe.UpdatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_HandlesNullOptionalFields()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForRecipeAsync(db).Returns("minimal");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("minimal");
        
        var handler = new CreateRecipeHandler(db, publicIdService, slugService);
        var command = new CreateRecipeCommand("Minimal", null, null, null, null, null, null, "user1");
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedRecipe = await db.Recipes.FirstOrDefaultAsync(r => r.PublicId == "minimal");
        Assert.NotNull(savedRecipe);
        Assert.Null(savedRecipe.Description);
        Assert.Null(savedRecipe.Ingredients);
        Assert.Null(savedRecipe.Instructions);
        Assert.Null(savedRecipe.PrepTime);
        Assert.Null(savedRecipe.CookTime);
        Assert.Null(savedRecipe.Servings);
    }
}
