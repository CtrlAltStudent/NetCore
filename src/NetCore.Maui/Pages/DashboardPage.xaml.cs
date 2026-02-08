using NetCore.Maui.Services;

#if !WINDOWS
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
#endif

namespace NetCore.Maui.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ApiClient _api;
    private Guid? _currentPeriodId;
    private List<(Guid Id, string Label)> _periods = new();

#if !WINDOWS
    private LiveChartsCore.SkiaSharpView.Maui.CartesianChart? _trendChart;
#endif

    public DashboardPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
#if WINDOWS
        TrendChartHost.Content = new Label { Text = "Wykresy niedostępne na Windows.", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        ChartUnsupportedLabel.IsVisible = true;
#else
        _trendChart = new LiveChartsCore.SkiaSharpView.Maui.CartesianChart { HeightRequest = 240, IsVisible = false };
        TrendChartHost.Content = _trendChart;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPeriodsAsync();
        await LoadReportAsync();
        await LoadTrendChartAsync();
    }

    private async Task LoadPeriodsAsync()
    {
        try
        {
            var periods = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
            _periods = periods?.Select(p => (p.Id, p.Label)).ToList() ?? new();
            PeriodPicker.ItemsSource = _periods.Select(x => x.Label).ToList();
            if (_periods.Count > 0)
                PeriodPicker.SelectedIndex = 0;
        }
        catch { /* ignore */ }
    }

    private void OnPeriodPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (PeriodPicker.SelectedIndex >= 0 && PeriodPicker.SelectedIndex < _periods.Count)
        {
            _currentPeriodId = _periods[PeriodPicker.SelectedIndex].Id;
            _ = LoadReportAsync();
        }
    }

    private async void OnSeedTestDataClicked(object? sender, EventArgs e)
    {
        SeedButton.IsEnabled = false;
        try
        {
            var response = await _api.PostAsJsonAsync("/api/v1/seed/test-data?force=true", new { });
            if (response.IsSuccessStatusCode)
            {
                MessageLabel.IsVisible = false;
                await LoadPeriodsAsync();
                await LoadReportAsync();
                await LoadTrendChartAsync();
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
        ReportLoadingIndicator.IsRunning = true;
        ReportLoadingIndicator.IsVisible = true;
        if (_periods.Count == 0)
        {
            ReportLoadingIndicator.IsRunning = false;
            ReportLoadingIndicator.IsVisible = false;
            MessageLabel.Text = "Brak okresów. Dodaj okres w konfiguracji.";
            MessageLabel.IsVisible = true;
            RevenueLabel.Text = "—";
            CostsLabel.Text = "—";
            ProfitLabel.Text = "—";
            MarginLabel.Text = "—";
            return;
        }
        var idx = PeriodPicker.SelectedIndex >= 0 ? PeriodPicker.SelectedIndex : 0;
        if (idx < 0 || idx >= _periods.Count) return;
        _currentPeriodId = _periods[idx].Id;
        PeriodLabel.Text = $"Okres: {_periods[idx].Label}";
        try
        {
            var report = await _api.GetFromJsonAsync<OperatingResultDto>($"/api/v1/reports/operating-result?periodId={_currentPeriodId}");
            if (report == null)
            {
                MessageLabel.Text = "Brak danych dla wybranego okresu.";
                MessageLabel.IsVisible = true;
                return;
            }
            RevenueLabel.Text = $"{report.TotalRevenue:N2} PLN";
            CostsLabel.Text = $"{report.TotalCosts:N2} PLN";
            ProfitLabel.Text = $"{report.OperatingProfit:N2} PLN";
            MarginLabel.Text = $"{report.MarginPercent:N1}%";
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Nie udało się załadować raportu. " + ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            ReportLoadingIndicator.IsRunning = false;
            ReportLoadingIndicator.IsVisible = false;
        }
    }

    private async Task LoadTrendChartAsync()
    {
        ChartLoadingIndicator.IsRunning = true;
        ChartLoadingIndicator.IsVisible = true;
        TrendChartHost.IsVisible = false;
        ChartNoDataLabel.IsVisible = false;
#if WINDOWS
        ChartUnsupportedLabel.IsVisible = true;
        ChartLoadingIndicator.IsRunning = false;
        ChartLoadingIndicator.IsVisible = false;
        return;
#else
        try
        {
            if (_trendChart == null) return;
            var list = await _api.GetFromJsonAsync<List<ReportSeriesItemDto>>("/api/v1/reports/series?count=6");
            if (list == null || list.Count == 0)
            {
                ChartNoDataLabel.IsVisible = true;
                return;
            }
            var labels = list.Select(x => x.Label).ToArray();
            var revenueValues = list.Select(x => (double)x.TotalRevenue).ToArray();
            var costValues = list.Select(x => (double)x.TotalCosts).ToArray();
            var profitValues = list.Select(x => (double)x.OperatingProfit).ToArray();

            _trendChart.Series = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = revenueValues,
                    Name = "Przychody",
                    Fill = new SolidColorPaint(SKColors.Green),
                    Stroke = null,
                },
                new ColumnSeries<double>
                {
                    Values = costValues,
                    Name = "Koszty",
                    Fill = new SolidColorPaint(SKColors.Orange),
                    Stroke = null,
                },
                new ColumnSeries<double>
                {
                    Values = profitValues,
                    Name = "Zysk",
                    Fill = new SolidColorPaint(SKColors.DodgerBlue),
                    Stroke = null,
                },
            };
            _trendChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                }
            };
            _trendChart.YAxes = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0"),
                }
            };
            _trendChart.IsVisible = true;
            TrendChartHost.IsVisible = true;
        }
        catch
        {
            ChartNoDataLabel.Text = "Nie udało się załadować trendu.";
            ChartNoDataLabel.IsVisible = true;
        }
        finally
        {
            ChartLoadingIndicator.IsRunning = false;
            ChartLoadingIndicator.IsVisible = false;
        }
#endif
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        if (!_currentPeriodId.HasValue)
        {
            await DisplayAlertAsync("Eksport", "Wybierz okres (załadowany raport).", "OK");
            return;
        }
        try
        {
            var report = await _api.GetFromJsonAsync<OperatingResultDto>($"/api/v1/reports/operating-result?periodId={_currentPeriodId.Value}");
            if (report == null) { await DisplayAlertAsync("Eksport", "Brak danych do eksportu.", "OK"); return; }
            var csv = "Przychody;Koszty;Zysk operacyjny;Marża %\n"
                + $"{report.TotalRevenue:N2};{report.TotalCosts:N2};{report.OperatingProfit:N2};{report.MarginPercent:N1}";
            var path = Path.Combine(FileSystem.CacheDirectory, "raport_operacyjny.csv");
            await File.WriteAllTextAsync(path, csv, System.Text.Encoding.UTF8);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(path),
                Title = "Eksport raportu"
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
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

    private class ReportSeriesItemDto
    {
        public Guid PeriodId { get; set; }
        public string Label { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal OperatingProfit { get; set; }
        public decimal MarginPercent { get; set; }
    }
}
