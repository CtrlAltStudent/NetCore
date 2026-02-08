using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class RevenueEditPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly RevenuesPage.RevenueRow? _existing;
    private readonly List<Guid> _periodIds;
    private readonly List<Guid> _channelIds;

    public RevenueEditPage(ApiClient api, RevenuesPage.RevenueRow? existing, List<Guid> periodIds, List<string> periodLabels, List<Guid> channelIds, List<string> channelNames)
    {
        _api = api;
        _existing = existing;
        _periodIds = periodIds;
        _channelIds = channelIds;
        InitializeComponent();
        PeriodPicker.ItemsSource = periodLabels;
        ChannelPicker.ItemsSource = new[] { "(brak)" }.Concat(channelNames).ToList();
        if (existing != null)
        {
            var periodIdx = periodIds.IndexOf(existing.PeriodId);
            if (periodIdx >= 0) PeriodPicker.SelectedIndex = periodIdx;
            if (existing.ChannelId.HasValue)
            {
                var chIdx = channelIds.IndexOf(existing.ChannelId.Value);
                if (chIdx >= 0) ChannelPicker.SelectedIndex = chIdx + 1;
            }
            else
                ChannelPicker.SelectedIndex = 0;
            AmountEntry.Text = existing.Amount.ToString("G29");
            CurrencyEntry.Text = existing.Currency ?? "PLN";
            DatePicker.Date = existing.Date;
        }
        else
        {
            if (periodLabels.Count > 0) PeriodPicker.SelectedIndex = 0;
            ChannelPicker.SelectedIndex = 0;
            DatePicker.Date = DateTime.Today;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Navigation.PopModalAsync();

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (PeriodPicker.SelectedIndex < 0 || PeriodPicker.SelectedIndex >= _periodIds.Count)
        {
            await DisplayAlertAsync("Błąd", "Wybierz okres.", "OK");
            return;
        }
        if (!decimal.TryParse(AmountEntry.Text?.Replace(",", "."), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount < 0)
        {
            await DisplayAlertAsync("Błąd", "Podaj poprawną kwotę.", "OK");
            return;
        }
        var periodId = _periodIds[PeriodPicker.SelectedIndex];
        Guid? channelId = null;
        if (ChannelPicker.SelectedIndex > 0 && ChannelPicker.SelectedIndex <= _channelIds.Count)
            channelId = _channelIds[ChannelPicker.SelectedIndex - 1];
        var currency = CurrencyEntry.Text?.Trim() ?? "PLN";
        var date = DatePicker.Date;

        try
        {
            if (_existing != null)
            {
                var res = await _api.PutAsJsonAsync($"/api/v1/revenues/{_existing.Id}", new { ChannelId = channelId, PeriodId = periodId, Amount = amount, Currency = currency, Date = date });
                if (!res.IsSuccessStatusCode) { await DisplayAlertAsync("Błąd", "Nie udało się zapisać.", "OK"); return; }
            }
            else
            {
                var res = await _api.PostAsJsonAsync("/api/v1/revenues", new { ChannelId = channelId, PeriodId = periodId, Amount = amount, Currency = currency, Date = date });
                if (!res.IsSuccessStatusCode) { await DisplayAlertAsync("Błąd", "Nie udało się dodać.", "OK"); return; }
            }
            await Navigation.PopModalAsync();
        }
        catch (Exception ex) { await DisplayAlertAsync("Błąd", ex.Message, "OK"); }
    }
}
