using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NetCore.Maui.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;
    private readonly string _baseUrl;

    public ApiClient(HttpClient http, AuthService auth, string baseUrl)
    {
        _http = http;
        _auth = auth;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    private async Task EnsureTokenAsync()
    {
        var token = await _auth.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HttpResponseMessage> GetAsync(string path)
    {
        await EnsureTokenAsync();
        return await _http.GetAsync($"{_baseUrl}{path}");
    }

    public async Task<T?> GetFromJsonAsync<T>(string path)
    {
        await EnsureTokenAsync();
        return await _http.GetFromJsonAsync<T>($"{_baseUrl}{path}");
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string path, T value)
    {
        await EnsureTokenAsync();
        return await _http.PostAsJsonAsync($"{_baseUrl}{path}", value);
    }
}
