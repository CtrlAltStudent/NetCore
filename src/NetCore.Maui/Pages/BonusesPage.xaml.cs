using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class BonusesPage : ContentPage
{
    private readonly ApiClient _api;
    private List<(Guid Id, string Label)> _periods = new();

    public BonusesPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var periods = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
        _periods = periods?.Select(p => (p.Id, p.Label)).ToList() ?? new();
        PeriodPicker.ItemsSource = _periods.Select(x => x.Label).ToList();
        if (_periods.Count > 0)
            PeriodPicker.SelectedIndex = 0;
    }

    private async void OnCalculateClicked(object? sender, EventArgs e)
    {
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0) return;
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        try
        {
            var results = await _api.GetFromJsonAsync<List<BonusDto>>($"/api/v1/bonuses/calculate?periodId={periodId}");
            var items = results?.Select(r => new BonusDisplayItem(r.EmployeeName, r.Details, $"{r.Amount:N2} PLN")).ToList() ?? new List<BonusDisplayItem>();
            ResultsList.ItemsSource = items;
        }
        catch
        {
            ResultsList.ItemsSource = new List<BonusDisplayItem>();
        }
    }

    private record BonusDisplayItem(string EmployeeName, string Details, string AmountText);
    private class PeriodDto { public Guid Id { get; set; } public string Label { get; set; } = ""; }
    private class BonusDto { public string EmployeeName { get; set; } = ""; public string Details { get; set; } = ""; public decimal Amount { get; set; } }
}
