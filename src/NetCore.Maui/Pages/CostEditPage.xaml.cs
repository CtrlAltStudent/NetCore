using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class CostEditPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly CostsPage.CostRow? _existing;
    private readonly List<Guid> _periodIds;

    public CostEditPage(ApiClient api, CostsPage.CostRow? existing, List<Guid> periodIds, List<string> periodLabels)
    {
        _api = api;
        _existing = existing;
        _periodIds = periodIds;
        InitializeComponent();
        PeriodPicker.ItemsSource = periodLabels;
        TypePicker.ItemsSource = new[] { "Stały", "Zmienny" };
        if (existing != null)
        {
            var periodIdx = periodIds.IndexOf(existing.PeriodId);
            if (periodIdx >= 0) PeriodPicker.SelectedIndex = periodIdx;
            TypePicker.SelectedIndex = existing.Type;
            NameEntry.Text = existing.Name;
            AmountEntry.Text = existing.Amount.ToString("G29");
            CurrencyEntry.Text = existing.Currency ?? "PLN";
        }
        else
        {
            if (periodLabels.Count > 0) PeriodPicker.SelectedIndex = 0;
            TypePicker.SelectedIndex = 0;
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
        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Błąd", "Podaj nazwę kosztu.", "OK");
            return;
        }
        if (!decimal.TryParse(AmountEntry.Text?.Replace(",", "."), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount < 0)
        {
            await DisplayAlertAsync("Błąd", "Podaj poprawną kwotę.", "OK");
            return;
        }
        var periodId = _periodIds[PeriodPicker.SelectedIndex];
        var type = TypePicker.SelectedIndex;
        if (type < 0) type = 0;
        var currency = CurrencyEntry.Text?.Trim() ?? "PLN";

        try
        {
            if (_existing != null)
            {
                var res = await _api.PutAsJsonAsync($"/api/v1/costs/{_existing.Id}", new { Type = type, Name = name, Amount = amount, PeriodId = periodId, Currency = currency, Assignments = (List<object>?)null });
                if (!res.IsSuccessStatusCode) { await DisplayAlertAsync("Błąd", "Nie udało się zapisać.", "OK"); return; }
            }
            else
            {
                var res = await _api.PostAsJsonAsync("/api/v1/costs", new { Type = type, Name = name, Amount = amount, PeriodId = periodId, Currency = currency, Assignments = (List<object>?)null });
                if (!res.IsSuccessStatusCode) { await DisplayAlertAsync("Błąd", "Nie udało się dodać.", "OK"); return; }
            }
            await Navigation.PopModalAsync();
        }
        catch (Exception ex) { await DisplayAlertAsync("Błąd", ex.Message, "OK"); }
    }
}
