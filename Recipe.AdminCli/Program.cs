using System.CommandLine;
using Recipe.AdminCli.Auth;
using Recipe.AdminCli.Commands;
using Recipe.AdminCli.Services;
using Recipe.AdminCli.Storage;

// Configuration - would normally come from appsettings
const string ApiBaseUrl = "http://localhost:5001"; // Default AdminApi URL
const string OidcAuthority = "https://dev-oidc-provider.example.com";
const string OidcClientId = "recipe-admin-cli";
var scopes = new[] { "recipe-admin-api" };

// Choose storage based on environment
var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development";

ITokenStorage tokenStorage = isDevelopment
    ? new FileTokenStorage()
    : new SecureTokenStorage();

var authService = new OidcAuthService(tokenStorage, OidcClientId, OidcAuthority, scopes);
var apiClient = new AdminApiClient(ApiBaseUrl);
var userCommands = new UserCommands(apiClient, authService);

// Build CLI
var rootCommand = new RootCommand("Recipe Admin CLI - Manage users and permissions");

// Login command
var loginCommand = new Command("login", "Authenticate using OIDC device flow");
loginCommand.SetHandler(async () =>
{
    Console.WriteLine("Starting authentication...");
    var token = await authService.LoginWithDeviceCodeAsync();
    if (token != null)
    {
        Console.WriteLine("Authentication successful!");
    }
    else
    {
        Console.WriteLine("Authentication failed.");
    }
});
rootCommand.AddCommand(loginCommand);

// Logout command
var logoutCommand = new Command("logout", "Clear cached authentication token");
logoutCommand.SetHandler(async () =>
{
    await authService.LogoutAsync();
});
rootCommand.AddCommand(logoutCommand);

// User commands group
var userCommand = new Command("user", "User management commands");

// Search users
var searchCommand = new Command("search", "Search for users");
var searchTermOption = new Option<string?>("--term", "Search term for username, email, or display name");
var pageOption = new Option<int>("--page", () => 1, "Page number");
var pageSizeOption = new Option<int>("--page-size", () => 20, "Page size");
searchCommand.AddOption(searchTermOption);
searchCommand.AddOption(pageOption);
searchCommand.AddOption(pageSizeOption);
searchCommand.SetHandler(async (string? term, int page, int pageSize) =>
{
    await userCommands.SearchUsersAsync(term, page, pageSize);
}, searchTermOption, pageOption, pageSizeOption);
userCommand.AddCommand(searchCommand);

// Get user details
var detailsCommand = new Command("details", "Get detailed user information");
var userIdArgument = new Argument<string>("user-id", "User ID");
detailsCommand.AddArgument(userIdArgument);
detailsCommand.SetHandler(async (string userId) =>
{
    await userCommands.GetUserDetailsAsync(userId);
}, userIdArgument);
userCommand.AddCommand(detailsCommand);

// Enable user access
var enableCommand = new Command("enable", "Enable user access");
var enableUserIdArg = new Argument<string>("user-id", "User ID");
enableCommand.AddArgument(enableUserIdArg);
enableCommand.SetHandler(async (string userId) =>
{
    await userCommands.EnableUserAsync(userId);
}, enableUserIdArg);
userCommand.AddCommand(enableCommand);

// Disable user access
var disableCommand = new Command("disable", "Disable user access");
var disableUserIdArg = new Argument<string>("user-id", "User ID");
disableCommand.AddArgument(disableUserIdArg);
disableCommand.SetHandler(async (string userId) =>
{
    await userCommands.DisableUserAsync(userId);
}, disableUserIdArg);
userCommand.AddCommand(disableCommand);

// Assign admin role
var assignAdminCommand = new Command("assign-admin", "Assign admin role to user");
var assignUserIdArg = new Argument<string>("user-id", "User ID");
assignAdminCommand.AddArgument(assignUserIdArg);
assignAdminCommand.SetHandler(async (string userId) =>
{
    await userCommands.AssignAdminRoleAsync(userId);
}, assignUserIdArg);
userCommand.AddCommand(assignAdminCommand);

// Remove admin role
var removeAdminCommand = new Command("remove-admin", "Remove admin role from user");
var removeUserIdArg = new Argument<string>("user-id", "User ID");
removeAdminCommand.AddArgument(removeUserIdArg);
removeAdminCommand.SetHandler(async (string userId) =>
{
    await userCommands.RemoveAdminRoleAsync(userId);
}, removeUserIdArg);
userCommand.AddCommand(removeAdminCommand);

rootCommand.AddCommand(userCommand);

// Execute
return await rootCommand.InvokeAsync(args);
