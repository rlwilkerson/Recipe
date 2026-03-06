namespace Recipe.AdminCli.Storage;

public interface ITokenStorage
{
    Task<string?> GetTokenAsync();
    Task SaveTokenAsync(string token);
    Task ClearTokenAsync();
}
