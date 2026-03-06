using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Recipe.AdminApi.Data;
using Recipe.AdminApi.Features.Users.SearchUsers;
using Recipe.Tests.TestHelpers;

namespace Recipe.Tests.Features.Admin.SearchUsers;

public class SearchUsersHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllUsers_WhenNoSearchTerm()
    {
        // Arrange
        var db = CreateDbContext();
        var userManager = CreateUserManager(db);
        
        var user1 = new ApplicationUser { Id = "1", UserName = "alice", Email = "alice@test.com" };
        var user2 = new ApplicationUser { Id = "2", UserName = "bob", Email = "bob@test.com" };
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();
        
        var handler = new SearchUsersHandler(db, userManager);
        var query = new SearchUsersQuery(null, 1, 20);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Users.Count);
    }

    [Fact]
    public async Task Handle_FiltersUsers_WhenSearchTermProvided()
    {
        // Arrange
        var db = CreateDbContext();
        var userManager = CreateUserManager(db);
        
        var user1 = new ApplicationUser { Id = "1", UserName = "alice", Email = "alice@test.com" };
        var user2 = new ApplicationUser { Id = "2", UserName = "bob", Email = "bob@test.com" };
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();
        
        var handler = new SearchUsersHandler(db, userManager);
        var query = new SearchUsersQuery("alice", 1, 20);
        
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Users);
        Assert.Equal("alice", result.Users[0].UserName);
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private UserManager<ApplicationUser> CreateUserManager(AppDbContext db)
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);
        
        userManager.IsInRoleAsync(Arg.Any<ApplicationUser>(), "Admin")
            .Returns(Task.FromResult(false));
        
        return userManager;
    }
}
