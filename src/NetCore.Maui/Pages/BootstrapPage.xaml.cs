using Microsoft.Extensions.DependencyInjection;

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
        var services = Handler?.MauiContext?.Services;
        if (services == null)
        {
            if (Application.Current?.Windows.Count > 0)
                Application.Current.Windows[0].Page = new ContentPage { Content = new Label { Text = "Błąd inicjalizacji." } };
            return;
        }
        var auth = services.GetRequiredService<NetCore.Maui.Services.AuthService>();
        var loggedIn = await auth.IsLoggedInAsync();
        var window = Application.Current?.Windows.Count > 0 ? Application.Current!.Windows[0] : null;
        if (window != null)
            window.Page = loggedIn ? services.GetRequiredService<AppShell>() : new LoginPage(auth);
    }
}
