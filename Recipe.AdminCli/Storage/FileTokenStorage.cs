namespace Recipe.AdminCli.Storage;

/// <summary>
/// Simple file-based token storage for local development
/// </summary>
public class FileTokenStorage : ITokenStorage
{
    private readonly string _tokenFilePath;

    public FileTokenStorage()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDir = Path.Combine(appDataPath, "RecipeAdminCli");
        Directory.CreateDirectory(appDir);
        _tokenFilePath = Path.Combine(appDir, "token.txt");
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!File.Exists(_tokenFilePath))
            return null;

        try
        {
            return await File.ReadAllTextAsync(_tokenFilePath);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveTokenAsync(string token)
    {
        await File.WriteAllTextAsync(_tokenFilePath, token);
    }

    public Task ClearTokenAsync()
    {
        if (File.Exists(_tokenFilePath))
        {
            File.Delete(_tokenFilePath);
        }
        return Task.CompletedTask;
    }
}
