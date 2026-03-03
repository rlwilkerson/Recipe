using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Recipe.Tests.TestHelpers;
using Recipe.Web.Features.Cookbooks.CreateCookbook;
using Recipe.Web.Services;

namespace Recipe.Tests.Features.Cookbooks;

public class CreateCookbookHandlerTests
{
    [Fact]
    public async Task Handle_CreatesCookbookWithAllFields()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("testPubId");
        slugService.GenerateSlug("My Cookbook").Returns("my-cookbook");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("My Cookbook", "Test description", "user123");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal("testPubId", response.PublicId);
        Assert.Equal("my-cookbook", response.Slug);
        Assert.Equal("My Cookbook", response.Name);
    }

    [Fact]
    public async Task Handle_SavesCookbookToDatabase()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("abc123xyz");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("test-slug");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("Test", null, "owner1");
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedCookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.PublicId == "abc123xyz");
        Assert.NotNull(savedCookbook);
        Assert.Equal("Test", savedCookbook.Name);
        Assert.Equal("owner1", savedCookbook.OwnerId);
        Assert.Equal("test-slug", savedCookbook.Slug);
    }

    [Fact]
    public async Task Handle_SetsCreatedAtTimestamp()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("id123");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("Test", null, "user1");
        
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        var afterCreate = DateTime.UtcNow;
        
        // Assert
        var savedCookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.PublicId == "id123");
        Assert.NotNull(savedCookbook);
        Assert.InRange(savedCookbook.CreatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_SetsUpdatedAtTimestamp()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("id456");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("Test", null, "user1");
        
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        var afterCreate = DateTime.UtcNow;
        
        // Assert
        var savedCookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.PublicId == "id456");
        Assert.NotNull(savedCookbook);
        Assert.InRange(savedCookbook.UpdatedAt, beforeCreate.AddSeconds(-1), afterCreate.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_UsesPublicIdServiceToGenerateId()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("uniqueId99");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("Test", null, "user1");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await publicIdService.Received(1).GenerateForCookbookAsync(db);
        Assert.Equal("uniqueId99", response.PublicId);
    }

    [Fact]
    public async Task Handle_UsesSlugServiceToGenerateSlug()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("id");
        slugService.GenerateSlug("My Test Cookbook").Returns("my-test-cookbook");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("My Test Cookbook", null, "user1");
        
        // Act
        var response = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        slugService.Received(1).GenerateSlug("My Test Cookbook");
        Assert.Equal("my-test-cookbook", response.Slug);
    }

    [Fact]
    public async Task Handle_StoresDescriptionCorrectly()
    {
        // Arrange
        var db = DbContextHelper.CreateInMemoryDbContext();
        var publicIdService = Substitute.For<IPublicIdService>();
        var slugService = Substitute.For<ISlugService>();
        
        publicIdService.GenerateForCookbookAsync(db).Returns("descId");
        slugService.GenerateSlug(Arg.Any<string>()).Returns("slug");
        
        var handler = new CreateCookbookHandler(db, publicIdService, slugService);
        var command = new CreateCookbookCommand("Test", "A detailed description", "user1");
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedCookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.PublicId == "descId");
        Assert.NotNull(savedCookbook);
        Assert.Equal("A detailed description", savedCookbook.Description);
    }
}
