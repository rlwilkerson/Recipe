using System.Security.Cryptography;
using System.Text;

namespace Recipe.AdminCli.Storage;

/// <summary>
/// Production token storage using OS credential store via DPAPI on Windows
/// </summary>
public class SecureTokenStorage : ITokenStorage
{
    private readonly string _keyName = "RecipeAdminCli.AccessToken";

    public Task<string?> GetTokenAsync()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var protectedData = LoadFromWindowsRegistry();
                if (protectedData != null)
                {
                    var unprotectedData = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                    return Task.FromResult<string?>(Encoding.UTF8.GetString(unprotectedData));
                }
            }
            return Task.FromResult<string?>(null);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task SaveTokenAsync(string token)
    {
        if (OperatingSystem.IsWindows())
        {
            var data = Encoding.UTF8.GetBytes(token);
            var protectedData = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            SaveToWindowsRegistry(protectedData);
        }
        return Task.CompletedTask;
    }

    public Task ClearTokenAsync()
    {
        if (OperatingSystem.IsWindows())
        {
            DeleteFromWindowsRegistry();
        }
        return Task.CompletedTask;
    }

    private byte[]? LoadFromWindowsRegistry()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\RecipeAdminCli");
        if (key == null) return null;
        var value = key.GetValue(_keyName) as byte[];
        return value;
    }

    private void SaveToWindowsRegistry(byte[] data)
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\RecipeAdminCli");
        key.SetValue(_keyName, data, Microsoft.Win32.RegistryValueKind.Binary);
    }

    private void DeleteFromWindowsRegistry()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\RecipeAdminCli", writable: true);
        key?.DeleteValue(_keyName, throwOnMissingValue: false);
    }
}
