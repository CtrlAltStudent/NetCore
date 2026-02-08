using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace NetCore.Maui.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;
    private readonly ApiBaseUrlService _baseUrlService;

    public ApiClient(HttpClient http, AuthService auth, ApiBaseUrlService baseUrlService)
    {
        _http = http;
        _auth = auth;
        _baseUrlService = baseUrlService;
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
        return await _http.GetAsync($"{_baseUrlService.GetBaseUrl()}{path}");
    }

    public async Task<T?> GetFromJsonAsync<T>(string path)
    {
        await EnsureTokenAsync();
        return await _http.GetFromJsonAsync<T>($"{_baseUrlService.GetBaseUrl()}{path}");
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string path, T value)
    {
        await EnsureTokenAsync();
        return await _http.PostAsJsonAsync($"{_baseUrlService.GetBaseUrl()}{path}", value);
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string path, T value)
    {
        await EnsureTokenAsync();
        return await _http.PutAsJsonAsync($"{_baseUrlService.GetBaseUrl()}{path}", value);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string path)
    {
        await EnsureTokenAsync();
        return await _http.DeleteAsync($"{_baseUrlService.GetBaseUrl()}{path}");
    }
}
