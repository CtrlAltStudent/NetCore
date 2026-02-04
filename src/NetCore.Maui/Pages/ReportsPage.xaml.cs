using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class ReportsPage : ContentPage
{
    private readonly ApiClient _api;
    private List<(Guid Id, string Label)> _periods = new();

    public ReportsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
        PeriodPicker.SelectedIndexChanged += async (_, _) => await LoadReportAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var periods = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
        _periods = periods?.Select(p => (p.Id, p.Label)).ToList() ?? new();
        PeriodPicker.ItemsSource = _periods.Select(x => x.Label).ToList();
        if (_periods.Count > 0)
            PeriodPicker.SelectedIndex = 0;
        await LoadReportAsync();
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

    private class PeriodDto { public Guid Id { get; set; } public string Label { get; set; } = ""; }
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
