using System.Net.Http.Json;

namespace NetCore.Maui.Services;

public class AuthService
{
    private const string TokenKey = "NetCore_JwtToken";
    private readonly HttpClient _http;
    private readonly ApiBaseUrlService _baseUrlService;

    public AuthService(HttpClient http, ApiBaseUrlService baseUrlService)
    {
        _http = http;
        _baseUrlService = baseUrlService;
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    /// <summary>Wynik logowania: Success + dane, albo błąd (ConnectionError / InvalidCredentials).</summary>
    public class LoginResult
    {
        public bool Success => Data != null;
        public AuthResult? Data { get; set; }
        public string ErrorKind { get; set; } = ""; // "ConnectionError" | "InvalidCredentials"
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{_baseUrlService.GetBaseUrl()}/api/v1/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode)
                return new LoginResult { ErrorKind = "InvalidCredentials" };
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth?.Token == null)
                return new LoginResult { ErrorKind = "InvalidCredentials" };
            await SecureStorage.Default.SetAsync(TokenKey, auth.Token);
            return new LoginResult { Data = new AuthResult { Token = auth.Token, Email = auth.Email, OrganizationId = auth.OrganizationId, Role = auth.Role } };
        }
        catch (Exception)
        {
            return new LoginResult { ErrorKind = "ConnectionError" };
        }
    }

    /// <summary>Rejestracja nowego użytkownika i organizacji. Przy sukcesie zapisuje token i zwraca dane jak przy logowaniu.</summary>
    public async Task<LoginResult> RegisterAsync(string email, string password, string organizationName)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"{_baseUrlService.GetBaseUrl()}/api/v1/auth/register", new { Email = email, Password = password, OrganizationName = organizationName ?? "" });
            if (!response.IsSuccessStatusCode)
                return new LoginResult { ErrorKind = "InvalidCredentials" };
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth?.Token == null)
                return new LoginResult { ErrorKind = "InvalidCredentials" };
            await SecureStorage.Default.SetAsync(TokenKey, auth.Token);
            return new LoginResult { Data = new AuthResult { Token = auth.Token, Email = auth.Email, OrganizationId = auth.OrganizationId, Role = auth.Role } };
        }
        catch (Exception)
        {
            return new LoginResult { ErrorKind = "ConnectionError" };
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
