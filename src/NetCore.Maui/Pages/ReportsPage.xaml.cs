using NetCore.Maui.Services;

#if !WINDOWS
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
#endif

namespace NetCore.Maui.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ApiClient _api;
    private List<(Guid Id, string Label)> _periods = new();
    private int _seriesCount = 12;

#if !WINDOWS
    private LiveChartsCore.SkiaSharpView.Maui.CartesianChart? _trendChart;
    private LiveChartsCore.SkiaSharpView.Maui.PieChart? _pieChart;
#endif

    public ReportsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
#if WINDOWS
        TrendChartHost.Content = new Label { Text = "Wykresy niedostępne na Windows.", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
        PieChartHost.Content = new Label { Text = "" };
        ChartsUnsupportedLabel.IsVisible = true;
#else
        _trendChart = new LiveChartsCore.SkiaSharpView.Maui.CartesianChart { HeightRequest = 240, IsVisible = false };
        _pieChart = new LiveChartsCore.SkiaSharpView.Maui.PieChart { HeightRequest = 220, IsVisible = false };
        TrendChartHost.Content = _trendChart;
        PieChartHost.Content = _pieChart;
#endif
        PeriodPicker.SelectedIndexChanged += async (_, _) =>
        {
            await LoadReportAsync();
            await LoadPieChartAsync();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var periods = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
        _periods = periods?.Select(p => (p.Id, p.Label)).ToList() ?? new();
        PeriodPicker.ItemsSource = _periods.Select(x => x.Label).ToList();
        if (_periods.Count > 0)
            PeriodPicker.SelectedIndex = 0;
        SeriesCountPicker.ItemsSource = new[] { "6 okresów", "12 okresów" };
        SeriesCountPicker.SelectedIndex = 1;
        await LoadReportAsync();
        await LoadTrendChartAsync();
        await LoadPieChartAsync();
    }

    private void OnSeriesCountChanged(object? sender, EventArgs e)
    {
        if (SeriesCountPicker.SelectedIndex >= 0)
            _seriesCount = SeriesCountPicker.SelectedIndex == 0 ? 6 : 12;
        _ = LoadTrendChartAsync();
    }

    private async Task LoadTrendChartAsync()
    {
        TrendLoadingIndicator.IsRunning = true;
        TrendLoadingIndicator.IsVisible = true;
        TrendChartHost.IsVisible = false;
        TrendNoDataLabel.IsVisible = false;
#if WINDOWS
        ChartsUnsupportedLabel.IsVisible = true;
        TrendLoadingIndicator.IsRunning = false;
        TrendLoadingIndicator.IsVisible = false;
        return;
#else
        try
        {
            if (_trendChart == null) return;
            var list = await _api.GetFromJsonAsync<List<ReportSeriesItemDto>>($"/api/v1/reports/series?count={_seriesCount}");
            if (list == null || list.Count == 0)
            {
                TrendNoDataLabel.IsVisible = true;
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
                new LiveChartsCore.SkiaSharpView.Axis
                {
                    Labels = labels,
                    LabelsRotation = 45,
                }
            };
            _trendChart.YAxes = new[]
            {
                new LiveChartsCore.SkiaSharpView.Axis { Labeler = value => value.ToString("N0") }
            };
            _trendChart.IsVisible = true;
            TrendChartHost.IsVisible = true;
        }
        catch
        {
            TrendNoDataLabel.Text = "Nie udało się załadować trendu.";
            TrendNoDataLabel.IsVisible = true;
        }
        finally
        {
            TrendLoadingIndicator.IsRunning = false;
            TrendLoadingIndicator.IsVisible = false;
        }
#endif
    }

    private async Task LoadPieChartAsync()
    {
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0)
        {
            PieChartHost.IsVisible = false;
            PieNoDataLabel.IsVisible = true;
            return;
        }
#if WINDOWS
        PieChartHost.IsVisible = false;
        return;
#else
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        PieLoadingIndicator.IsRunning = true;
        PieLoadingIndicator.IsVisible = true;
        PieChartHost.IsVisible = false;
        PieNoDataLabel.IsVisible = false;
        try
        {
            if (_pieChart == null) return;
            var margin = await _api.GetFromJsonAsync<MarginDto>($"/api/v1/reports/margin?periodId={periodId}");
            var byChannel = margin?.ByChannel?.Where(c => c.Revenue > 0).ToList() ?? new List<ChannelRow>();
            if (byChannel.Count == 0)
            {
                PieNoDataLabel.IsVisible = true;
                return;
            }
            var colors = new[] { SKColors.Green, SKColors.DodgerBlue, SKColors.Orange, SKColors.MediumPurple, SKColors.Teal };
            var series = new List<ISeries>();
            for (var i = 0; i < byChannel.Count; i++)
            {
                var c = byChannel[i];
                series.Add(new PieSeries<double>
                {
                    Values = new[] { (double)c.Revenue },
                    Name = c.ChannelName ?? "(bez kanału)",
                    Fill = new SolidColorPaint(colors[i % colors.Length]),
                });
            }
            _pieChart.Series = series;
            _pieChart.IsVisible = true;
            PieChartHost.IsVisible = true;
        }
        catch
        {
            PieNoDataLabel.Text = "Nie udało się załadować wykresu.";
            PieNoDataLabel.IsVisible = true;
        }
        finally
        {
            PieLoadingIndicator.IsRunning = false;
            PieLoadingIndicator.IsVisible = false;
        }
#endif
    }

    private async Task LoadReportAsync()
    {
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0) return;
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        try
        {
            var margin = await _api.GetFromJsonAsync<MarginDto>($"/api/v1/reports/margin?periodId={periodId}");
            if (margin == null) return;
            MarginSummary.Text = $"Przychody: {margin.TotalRevenue:N2} | Koszty: {margin.TotalCosts:N2} | Zysk: {margin.OperatingProfit:N2} | Marża: {margin.MarginPercent:N1}%";
            ByChannelList.ItemsSource = margin.ByChannel?.Select(c => new ChannelSummaryItem(c.ChannelName ?? "(bez kanału)", $"Przychód: {c.Revenue:N0} | Koszty: {c.Costs:N0} | Zysk: {c.Profit:N0}")).ToList() ?? new List<ChannelSummaryItem>();
            ByDepartmentList.ItemsSource = margin.ByDepartment?.Select(d => new DeptSummaryItem(d.DepartmentName ?? "(bez działu)", $"Koszty: {d.Costs:N0}")).ToList() ?? new List<DeptSummaryItem>();
        }
        catch { }
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0) { await DisplayAlertAsync("Eksport", "Wybierz okres.", "OK"); return; }
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        try
        {
            var margin = await _api.GetFromJsonAsync<MarginDto>($"/api/v1/reports/margin?periodId={periodId}");
            if (margin == null) { await DisplayAlertAsync("Eksport", "Brak danych.", "OK"); return; }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Przychody;Koszty;Zysk;Marża %");
            sb.AppendLine($"{margin.TotalRevenue:N2};{margin.TotalCosts:N2};{margin.OperatingProfit:N2};{margin.MarginPercent:N1}");
            sb.AppendLine();
            sb.AppendLine("Kanał;Przychód;Koszty;Zysk");
            foreach (var c in margin.ByChannel ?? new List<ChannelRow>())
                sb.AppendLine($"{c.ChannelName ?? ""};{c.Revenue:N2};{c.Costs:N2};{c.Profit:N2}");
            sb.AppendLine();
            sb.AppendLine("Dział;Koszty");
            foreach (var d in margin.ByDepartment ?? new List<DeptRow>())
                sb.AppendLine($"{d.DepartmentName ?? ""};{d.Costs:N2}");
            var path = Path.Combine(FileSystem.CacheDirectory, "raport_marza.csv");
            await File.WriteAllTextAsync(path, sb.ToString(), System.Text.Encoding.UTF8);
            await Share.Default.RequestAsync(new ShareFileRequest { File = new ShareFile(path), Title = "Eksport raportu" });
        }
        catch (Exception ex) { await DisplayAlertAsync("Błąd", ex.Message, "OK"); }
    }

    private class PeriodDto { public Guid Id { get; set; } public string Label { get; set; } = ""; }
    private class ReportSeriesItemDto
    {
        public string Label { get; set; } = "";
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal OperatingProfit { get; set; }
    }
    private class MarginDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal OperatingProfit { get; set; }
        public decimal MarginPercent { get; set; }
        public List<ChannelRow>? ByChannel { get; set; }
        public List<DeptRow>? ByDepartment { get; set; }
    }
    private class ChannelRow { public string? ChannelName { get; set; } public decimal Revenue { get; set; } public decimal Costs { get; set; } public decimal Profit { get; set; } }
    private class DeptRow { public string? DepartmentName { get; set; } public decimal Costs { get; set; } }
    private class ChannelSummaryItem
    {
        public string ChannelName { get; set; } = "";
        public string Summary { get; set; } = "";
        public ChannelSummaryItem(string n, string s) { ChannelName = n; Summary = s; }
    }
    private class DeptSummaryItem
    {
        public string DepartmentName { get; set; } = "";
        public string Summary { get; set; } = "";
        public DeptSummaryItem(string n, string s) { DepartmentName = n; Summary = s; }
    }
}
