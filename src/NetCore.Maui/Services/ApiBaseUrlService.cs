namespace NetCore.Maui.Services;

/// <summary>Dostarcza i zapisuje adres bazowy API (zapis w Preferences).</summary>
public class ApiBaseUrlService
{
    private const string Key = "NetCore_ApiBaseUrl";
    private const string DefaultUrl = "http://localhost:5174";

    public string GetBaseUrl()
    {
        return Preferences.Default.Get(Key, DefaultUrl).TrimEnd('/');
    }

    public async Task SetBaseUrlAsync(string url)
    {
        var value = (url ?? "").Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(value))
            value = DefaultUrl;
        Preferences.Default.Set(Key, value);
        await Task.CompletedTask;
    }
}
