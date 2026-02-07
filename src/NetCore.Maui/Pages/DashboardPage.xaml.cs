using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ApiClient _api;

    public DashboardPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReportAsync();
    }

    private async void OnSeedTestDataClicked(object? sender, EventArgs e)
    {
        SeedButton.IsEnabled = false;
        try
        {
            // force=true — załaduj dane nawet gdy organizacja ma już okresy (zastąpi je danymi testowymi)
            var response = await _api.PostAsJsonAsync("/api/v1/seed/test-data?force=true", new { });
            if (response.IsSuccessStatusCode)
            {
                MessageLabel.IsVisible = false;
                await LoadReportAsync();
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                var msg = GetErrorMessage((int)response.StatusCode, body);
                MessageLabel.Text = msg;
                MessageLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd połączenia: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            SeedButton.IsEnabled = true;
        }
    }

    private static string GetErrorMessage(int statusCode, string body)
    {
        if (statusCode == 401)
            return "Sesja wygasła. Wyloguj się i zaloguj ponownie.";
        if (statusCode == 404)
            return "Błąd 404: endpoint nie znaleziony. Zatrzymaj API (Ctrl+C), wykonaj w folderze NetCore.Api: dotnet build, potem dotnet run. W przeglądarce sprawdź http://localhost:5174/swagger — czy jest POST /api/v1/seed/test-data.";
        try
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);
            if (json.TryGetProperty("message", out var msg))
                return msg.GetString() ?? body;
        }
        catch { /* ignore */ }
        if (!string.IsNullOrWhiteSpace(body) && body.Length <= 300)
            return body;
        return $"Błąd ({statusCode}). " + (body.Length > 200 ? body[..200] + "…" : body);
    }

    private async Task LoadReportAsync()
    {
        MessageLabel.IsVisible = false;
        try
        {
            var periods = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
            if (periods == null || periods.Count == 0)
            {
                MessageLabel.Text = "Brak okresów. Dodaj okres w konfiguracji.";
                MessageLabel.IsVisible = true;
                return;
            }
            var period = periods.First();
            PeriodLabel.Text = $"Okres: {period.Label}";
            var report = await _api.GetFromJsonAsync<OperatingResultDto>($"/api/v1/reports/operating-result?periodId={period.Id}");
            if (report == null)
            {
                MessageLabel.Text = "Brak danych dla wybranego okresu.";
                MessageLabel.IsVisible = true;
                return;
            }
            RevenueLabel.Text = $"Przychody: {report.TotalRevenue:N2} PLN";
            CostsLabel.Text = $"Koszty: {report.TotalCosts:N2} PLN";
            ProfitLabel.Text = $"Zysk netto: {report.OperatingProfit:N2} PLN";
            MarginLabel.Text = $"Marża: {report.MarginPercent:N1}%";
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Nie udało się załadować raportu. " + ex.Message;
            MessageLabel.IsVisible = true;
        }
    }

    private class PeriodDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
    }

    private class OperatingResultDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal OperatingProfit { get; set; }
        public decimal MarginPercent { get; set; }
    }
}
