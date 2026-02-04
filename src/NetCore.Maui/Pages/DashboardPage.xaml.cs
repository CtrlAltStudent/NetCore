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

    private async Task LoadReportAsync()
    {
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
