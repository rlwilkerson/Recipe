using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Recipe.AdminApi.Data;
using Recipe.AdminApi.Features.Users.SetAdminRole;

namespace Recipe.Tests.Features.Admin.SetAdminRole;

public class SetAdminRoleHandlerTests
{
    [Fact]
    public async Task Handle_AssignsAdminRole_WhenAssignRoleIsTrue()
    {
        // Arrange
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        var user = new ApplicationUser { Id = "1", UserName = "alice" };
        
        userManager.FindByIdAsync("1").Returns(Task.FromResult(user));
        roleManager.RoleExistsAsync("Admin").Returns(Task.FromResult(true));
        userManager.AddToRoleAsync(user, "Admin").Returns(Task.FromResult(IdentityResult.Success));
        
        var handler = new SetAdminRoleHandler(userManager, roleManager);
        var command = new SetAdminRoleCommand("1", true);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("assigned", result.Message);
        await userManager.Received(1).AddToRoleAsync(user, "Admin");
    }

    [Fact]
    public async Task Handle_RemovesAdminRole_WhenAssignRoleIsFalse()
    {
        // Arrange
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        var user = new ApplicationUser { Id = "1", UserName = "alice" };
        
        userManager.FindByIdAsync("1").Returns(Task.FromResult(user));
        roleManager.RoleExistsAsync("Admin").Returns(Task.FromResult(true));
        userManager.RemoveFromRoleAsync(user, "Admin").Returns(Task.FromResult(IdentityResult.Success));
        
        var handler = new SetAdminRoleHandler(userManager, roleManager);
        var command = new SetAdminRoleCommand("1", false);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        Assert.Contains("removed", result.Message);
        await userManager.Received(1).RemoveFromRoleAsync(user, "Admin");
    }

    [Fact]
    public async Task Handle_CreatesAdminRole_IfNotExists()
    {
        // Arrange
        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager();
        var user = new ApplicationUser { Id = "1", UserName = "alice" };
        
        userManager.FindByIdAsync("1").Returns(Task.FromResult(user));
        roleManager.RoleExistsAsync("Admin").Returns(Task.FromResult(false));
        roleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));
        userManager.AddToRoleAsync(user, "Admin").Returns(Task.FromResult(IdentityResult.Success));
        
        var handler = new SetAdminRoleHandler(userManager, roleManager);
        var command = new SetAdminRoleCommand("1", true);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result.Success);
        await roleManager.Received(1).CreateAsync(Arg.Any<IdentityRole>());
    }

    private UserManager<ApplicationUser> CreateUserManager()
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);
    }

    private RoleManager<IdentityRole> CreateRoleManager()
    {
        var roleStore = Substitute.For<IRoleStore<IdentityRole>>();
        return Substitute.For<RoleManager<IdentityRole>>(
            roleStore, null, null, null, null);
    }
}
