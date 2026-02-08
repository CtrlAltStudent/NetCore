using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _auth;

    public RegisterPage(AuthService auth)
    {
        _auth = auth;
        InitializeComponent();
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";
        var orgName = OrganizationEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Wpisz email i hasło.";
            ErrorLabel.IsVisible = true;
            return;
        }

        RegisterButton.IsEnabled = false;
        try
        {
            var result = await _auth.RegisterAsync(email, password, orgName);
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
                    ? "Nie można połączyć z API. Uruchom API (dotnet run w folderze NetCore.Api)."
                    : "Rejestracja nie powiodła się (np. email już zajęty).";
                ErrorLabel.IsVisible = true;
            }
        }
        finally
        {
            RegisterButton.IsEnabled = true;
        }
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new LoginPage(_auth);
    }
}
