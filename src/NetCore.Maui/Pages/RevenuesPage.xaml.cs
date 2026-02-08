using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class RevenuesPage : ContentPage
{
    private readonly ApiClient _api;
    private List<PeriodItem> _periods = new();

    public RevenuesPage(ApiClient api)
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
        if (PeriodPicker.SelectedIndex < 0 || _periods.Count == 0) { List.ItemsSource = new List<RevenueRow>(); return; }
        var periodId = _periods[PeriodPicker.SelectedIndex].Id;
        try
        {
            var list = await _api.GetFromJsonAsync<List<RevenueDto>>($"/api/v1/revenues?periodId={periodId}");
            var channels = await _api.GetFromJsonAsync<List<ChannelItem>>("/api/v1/sales-channels") ?? new List<ChannelItem>();
            var channelNames = channels.ToDictionary(c => c.Id, c => c.Name);
            var rows = (list ?? new List<RevenueDto>()).Select(r => new RevenueRow
            {
                Id = r.Id,
                ChannelId = r.ChannelId,
                PeriodId = r.PeriodId,
                Amount = r.Amount,
                Currency = r.Currency ?? "PLN",
                Date = r.Date,
                DisplayText = $"{channelNames.GetValueOrDefault(r.ChannelId ?? Guid.Empty, "(bez kanału)")}: {r.Amount:N2} {r.Currency ?? "PLN"}",
                DateText = r.Date.ToString("d")
            }).ToList();
            List.ItemsSource = rows;
        }
        catch
        {
            List.ItemsSource = new List<RevenueRow>();
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        if (_periods.Count == 0) { await DisplayAlertAsync("Błąd", "Dodaj najpierw okres.", "OK"); return; }
        var channels = await GetChannelsAsync();
        var page = new RevenueEditPage(_api, null, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList(), channels.Select(c => c.Id).ToList(), channels.Select(c => c.Name).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not RevenueRow row) return;
        List.SelectedItem = null;
        var channels = await GetChannelsAsync();
        var page = new RevenueEditPage(_api, row, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList(), channels.Select(c => c.Id).ToList(), channels.Select(c => c.Name).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not RevenueRow row) return;
        _ = EditRowAsync(row);
    }

    private async Task EditRowAsync(RevenueRow row)
    {
        var channels = await GetChannelsAsync();
        var page = new RevenueEditPage(_api, row, _periods.Select(p => p.Id).ToList(), _periods.Select(p => p.Label).ToList(), channels.Select(c => c.Id).ToList(), channels.Select(c => c.Name).ToList());
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not RevenueRow row) return;
        var ok = await DisplayAlertAsync("Usuń przychód", "Czy na pewno usunąć?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/revenues/{row.Id}");
            if (res.IsSuccessStatusCode) await LoadAsync();
        }
        catch { }
    }

    private async Task<List<ChannelItem>> GetChannelsAsync() =>
        await _api.GetFromJsonAsync<List<ChannelItem>>("/api/v1/sales-channels") ?? new List<ChannelItem>();

    private class PeriodItem { public Guid Id { get; set; } public string Label { get; set; } = ""; }
    private class ChannelItem { public Guid Id { get; set; } public string Name { get; set; } = ""; }
    private class RevenueDto { public Guid Id { get; set; } public Guid? ChannelId { get; set; } public Guid PeriodId { get; set; } public decimal Amount { get; set; } public string? Currency { get; set; } public DateTime Date { get; set; } }

    public class RevenueRow
    {
        public Guid Id { get; set; }
        public Guid? ChannelId { get; set; }
        public Guid PeriodId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PLN";
        public DateTime Date { get; set; }
        public string DisplayText { get; set; } = "";
        public string DateText { get; set; } = "";
    }
}
