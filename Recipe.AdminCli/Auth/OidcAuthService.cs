using Microsoft.Identity.Client;
using Recipe.AdminCli.Storage;

namespace Recipe.AdminCli.Auth;

public class OidcAuthService
{
    private readonly ITokenStorage _tokenStorage;
    private readonly string _clientId;
    private readonly string _authority;
    private readonly string[] _scopes;

    public OidcAuthService(ITokenStorage tokenStorage, string clientId, string authority, string[] scopes)
    {
        _tokenStorage = tokenStorage;
        _clientId = clientId;
        _authority = authority;
        _scopes = scopes;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        // Try to get cached token
        var cachedToken = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(cachedToken))
        {
            // TODO: Validate token hasn't expired
            return cachedToken;
        }

        // Perform device code flow login
        return await LoginWithDeviceCodeAsync();
    }

    public async Task<string?> LoginWithDeviceCodeAsync()
    {
        try
        {
            var app = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(_authority)
                .Build();

            var result = await app.AcquireTokenWithDeviceCode(_scopes, deviceCodeResult =>
            {
                Console.WriteLine(deviceCodeResult.Message);
                return Task.CompletedTask;
            }).ExecuteAsync();

            // Save token
            await _tokenStorage.SaveTokenAsync(result.AccessToken);
            
            return result.AccessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearTokenAsync();
        Console.WriteLine("Logged out successfully.");
    }
}
