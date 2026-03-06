using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Recipe.AdminApi.Data;
using Recipe.AdminApi.Features.Users.SetUserAccess;

namespace Recipe.Tests.Features.Admin.SetUserAccess;

public class SetUserAccessHandlerTests
{
    [Fact]
    public async Task Handle_EnablesAccess_WhenEnableAccessIsTrue()
    {
        // Arrange
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "1", UserName = "alice" };
        
        userManager.FindByIdAsync("1").Returns(Task.FromResult(user));
        userManager.SetLockoutEndDateAsync(user, null).Returns(Task.FromResult(IdentityResult.Success));
        
        var handler = new SetUserAccessHandler(userManager);
        var command = new SetUserAccessCommand("1", true);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("enabled", result.Message);
        await userManager.Received(1).SetLockoutEndDateAsync(user, null);
    }

    [Fact]
    public async Task Handle_DisablesAccess_WhenEnableAccessIsFalse()
    {
        // Arrange
        var userManager = CreateUserManager();
        var user = new ApplicationUser { Id = "1", UserName = "alice" };
        
        userManager.FindByIdAsync("1").Returns(Task.FromResult(user));
        userManager.SetLockoutEndDateAsync(user, Arg.Any<DateTimeOffset?>()).Returns(Task.FromResult(IdentityResult.Success));
        
        var handler = new SetUserAccessHandler(userManager);
        var command = new SetUserAccessCommand("1", false);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("disabled", result.Message);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserNotFound()
    {
        // Arrange
        var userManager = CreateUserManager();
        userManager.FindByIdAsync("999").Returns(Task.FromResult<ApplicationUser>(null!));
        
        var handler = new SetUserAccessHandler(userManager);
        var command = new SetUserAccessCommand("999", true);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    private UserManager<ApplicationUser> CreateUserManager()
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);
    }
}
