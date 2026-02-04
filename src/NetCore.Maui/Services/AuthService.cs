using System.Net.Http.Json;

namespace NetCore.Maui.Services;

public class AuthService
{
    private const string TokenKey = "NetCore_JwtToken";
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public AuthService(HttpClient http, string baseUrl)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    public async Task<AuthResult?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{_baseUrl}/api/v1/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
                return null;
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth?.Token == null)
                return null;
            await SecureStorage.Default.SetAsync(TokenKey, auth.Token);
            return new AuthResult { Token = auth.Token, Email = auth.Email, OrganizationId = auth.OrganizationId, Role = auth.Role };
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        await Task.CompletedTask;
    }

    public class AuthResult
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        public Guid OrganizationId { get; set; }
        public string Role { get; set; } = "";
    }

    private class AuthResponse
    {
        public string Token { get; set; } = "";
        public string Email { get; set; } = "";
        public Guid OrganizationId { get; set; }
        public string Role { get; set; } = "";
    }
}
