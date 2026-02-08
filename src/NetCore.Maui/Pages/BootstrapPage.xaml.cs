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
        var services = App.Services ?? Handler?.MauiContext?.Services;
        if (services == null)
        {
            SetErrorContent("Błąd inicjalizacji: brak kontenera usług. Uruchom aplikację ponownie.");
            return;
        }
        try
        {
            var auth = services.GetRequiredService<AuthService>();
            var loggedIn = await auth.IsLoggedInAsync();
            var window = Application.Current?.Windows.Count > 0 ? Application.Current!.Windows[0] : null;
            if (window == null)
            {
                SetErrorContent("Brak okna aplikacji.");
                return;
            }

            if (loggedIn)
            {
                var api = services.GetRequiredService<ApiClient>();
                var check = await api.GetAsync("/api/v1/periods");
                if (check.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await auth.LogoutAsync();
                    SetWindowPage(window, new LoginPage(auth));
                    return;
                }
                SetWindowPage(window, services.GetRequiredService<AppShell>());
            }
            else
            {
                SetWindowPage(window, new LoginPage(auth));
            }
        }
        catch (Exception ex)
        {
            SetErrorContent("Błąd inicjalizacji: " + ex.Message);
        }
    }

    /// <summary>Pokazuje błąd na tej samej stronie (bez zamiany okna – unikamy crashy na Windows).</summary>
    public void SetErrorContent(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Padding = 24,
                BackgroundColor = Colors.White,
                Children =
                {
                    new Label { Text = message, TextColor = Colors.Red, MaxLines = 15 }
                }
            };
        });
    }

    /// <summary>Ustawia stronę okna na wątku UI; przy błędzie pokazuje komunikat na stronie bootstrap.</summary>
    private void SetWindowPage(Window window, Page page)
    {
        if (page is ContentPage cp && cp.BackgroundColor == Colors.Transparent)
            cp.BackgroundColor = Colors.White;
        var fallback = this;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                window.Page = page;
            }
            catch (Exception ex)
            {
                fallback.SetErrorContent("Nie udało się przełączyć strony: " + ex.Message);
            }
        });
    }
}
