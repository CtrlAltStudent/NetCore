using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly ApiBaseUrlService _baseUrlService;

    public SettingsPage(ApiBaseUrlService baseUrlService)
    {
        _baseUrlService = baseUrlService;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BaseUrlEntry.Text = _baseUrlService.GetBaseUrl();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var url = BaseUrlEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(url))
        {
            await DisplayAlertAsync("Błąd", "Podaj adres API.", "OK");
            return;
        }
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlertAsync("Błąd", "Adres musi zaczynać się od http:// lub https://", "OK");
            return;
        }
        await _baseUrlService.SetBaseUrlAsync(url);
        await DisplayAlertAsync("Zapisano", "Adres API został zapisany.", "OK");
    }
}
