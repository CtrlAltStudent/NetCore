using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class CostsPage : ContentPage
{
    private readonly ApiClient _api;
    private List<PeriodItem> _periods = new();

    public CostsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MessageLabel.IsVisible = false;
        var periods = await _api.GetFromJsonAsync<List<PeriodItem>>("/api/v1/periods");
        _periods = periods ?? new List<PeriodItem>();
        PeriodPicker.ItemsSource = _periods.Select(p => p.Label).ToList();
        if (_periods.Count > 0 && PeriodPicker.SelectedIndex < 0)
            PeriodPicker.SelectedIndex = 0;
        await LoadAsync();
    }

    private async void OnPeriodChanged(object? sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0) { List.ItemsSource = new List<CostRow>(); return; }
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        try
        {
            var list = await _api.GetFromJsonAsync<List<CostDto>>($"/api/v1/costs?periodId={periodId}");
            var rows = (list ?? new List<CostDto>()).Select(c => new CostRow
            {
                Id = c.Id,
                Type = c.Type,
                Name = c.Name,
                Amount = c.Amount,
                PeriodId = c.PeriodId,
                Currency = c.Currency ?? "PLN",
                DisplayText = $"{c.Name}: {c.Amount:N2} {c.Currency ?? "PLN"}",
                TypeText = c.Type == 0 ? "Stały" : "Zmienny"
            }).ToList();
            List.ItemsSource = rows;
        }
        catch
        {
            List.ItemsSource = new List<CostRow>();
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        if (_periods.Count == 0) { await DisplayAlertAsync("Błąd", "Dodaj najpierw okres.", "OK"); return; }
        var page = new CostEditPage(_api, null, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not CostRow row) return;
        List.SelectedItem = null;
        var page = new CostEditPage(_api, row, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not CostRow row) return;
        var page = new CostEditPage(_api, row, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not CostRow item) return;
        var ok = await DisplayAlertAsync("Usuń koszt", "Czy na pewno usunąć?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/costs/{item.Id}");
            if (res.IsSuccessStatusCode) await LoadAsync();
        }
        catch { }
    }

    private class PeriodItem { public Guid Id { get; set; } public string Label { get; set; } = ""; }
    private class CostDto { public Guid Id { get; set; } public int Type { get; set; } public string Name { get; set; } = ""; public decimal Amount { get; set; } public Guid PeriodId { get; set; } public string? Currency { get; set; } }

    public class CostRow
    {
        public Guid Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
        public Guid PeriodId { get; set; }
        public string Currency { get; set; } = "PLN";
        public string DisplayText { get; set; } = "";
        public string TypeText { get; set; } = "";
    }
}
