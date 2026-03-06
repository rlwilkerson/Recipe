using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Recipe.AdminCli.Services;

public class AdminApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AdminApiClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<SearchUsersResponse?> SearchUsersAsync(string? search, int page = 1, int pageSize = 20)
    {
        var url = $"/api/users/search?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SearchUsersResponse>();
    }

    public async Task<UserDetailsResponse?> GetUserDetailsAsync(string userId)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDetailsResponse>();
    }

    public async Task<ActionResponse?> SetUserAccessAsync(string userId, bool enableAccess)
    {
        var request = new { EnableAccess = enableAccess };
        var response = await _httpClient.PostAsJsonAsync($"/api/users/{userId}/access", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActionResponse>();
    }

    public async Task<ActionResponse?> SetAdminRoleAsync(string userId, bool assignRole)
    {
        var request = new { AssignRole = assignRole };
        var response = await _httpClient.PostAsJsonAsync($"/api/users/{userId}/admin-role", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ActionResponse>();
    }
}

// Response DTOs
public record SearchUsersResponse(List<UserSearchResult> Users, int TotalCount, int Page, int PageSize);
public record UserSearchResult(string Id, string? UserName, string? Email, string? DisplayName, bool IsLocked, bool IsAdmin, DateTimeOffset? LockoutEnd);
public record UserDetailsResponse(string Id, string? UserName, string? Email, string? DisplayName, bool EmailConfirmed, bool IsLocked, bool IsAdmin, DateTimeOffset? LockoutEnd, int RecipeCount, int CookbookCount);
public record ActionResponse(bool Success, string Message);
