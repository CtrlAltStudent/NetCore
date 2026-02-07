using Microsoft.Extensions.DependencyInjection;
using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _auth;

    public LoginPage(AuthService auth)
    {
        _auth = auth;
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Wpisz email i hasło.";
            ErrorLabel.IsVisible = true;
            return;
        }

        LoginButton.IsEnabled = false;
        try
        {
            var result = await _auth.LoginAsync(email, password);
            if (result.Success && result.Data != null)
            {
                var services = App.Services ?? Application.Current?.Handler?.MauiContext?.Services;
                var shell = services?.GetRequiredService<AppShell>();
                if (Application.Current?.Windows.Count > 0 && shell != null)
                    Application.Current.Windows[0].Page = shell;
            }
            else
            {
                ErrorLabel.Text = result.ErrorKind == "ConnectionError"
                    ? "Nie można połączyć z API. Uruchom najpierw API: w folderze src\\NetCore.Api wpisz dotnet run (w osobnym oknie)."
                    : "Nieprawidłowy email lub hasło.";
                ErrorLabel.IsVisible = true;
            }
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }
}
