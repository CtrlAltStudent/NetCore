using Microsoft.Extensions.DependencyInjection;
using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class BootstrapPage : ContentPage
{
    public BootstrapPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Na Windows Handler?.MauiContext?.Services bywa null przy pierwszym OnAppearing – użyj App.Services.
        var services = App.Services ?? Handler?.MauiContext?.Services;
        if (services == null)
        {
            ShowError("Błąd inicjalizacji: brak kontenera usług. Uruchom aplikację ponownie.");
            return;
        }
        try
        {
            var auth = services.GetRequiredService<AuthService>();
            var loggedIn = await auth.IsLoggedInAsync();
            var window = Application.Current?.Windows.Count > 0 ? Application.Current!.Windows[0] : null;
            if (window == null) return;

            if (loggedIn)
            {
                // Zweryfikuj, czy token jest jeszcze ważny (np. nie wygasł). Przy 401 traktuj jako wylogowanie.
                var api = services.GetRequiredService<ApiClient>();
                var check = await api.GetAsync("/api/v1/periods");
                if (check.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await auth.LogoutAsync();
                    window.Page = new LoginPage(auth);
                    return;
                }
                window.Page = services.GetRequiredService<AppShell>();
            }
            else
            {
                window.Page = new LoginPage(auth);
            }
        }
        catch (Exception ex)
        {
            ShowError("Błąd inicjalizacji: " + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        if (Application.Current?.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Padding = 24,
                    Children =
                    {
                        new Label { Text = message, TextColor = Colors.Red, MaxLines = 10 }
                    }
                }
            };
        }
    }
}
