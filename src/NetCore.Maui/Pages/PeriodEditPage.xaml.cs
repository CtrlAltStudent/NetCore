using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class PeriodEditPage : ContentPage
{
    private readonly ApiClient _api;
    private readonly Guid? _id;

    public PeriodEditPage(ApiClient api, PeriodDto? existing = null)
    {
        _api = api;
        _id = existing?.Id;
        InitializeComponent();
        if (existing != null)
        {
            LabelEntry.Text = existing.Label;
            StartDatePicker.Date = existing.StartDate;
            EndDatePicker.Date = existing.EndDate;
        }
        else
        {
            var today = DateTime.Today;
            StartDatePicker.Date = new DateTime(today.Year, today.Month, 1);
            EndDatePicker.Date = today;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var label = LabelEntry.Text?.Trim();
        if (string.IsNullOrEmpty(label))
        {
            await DisplayAlertAsync("Błąd", "Podaj nazwę okresu.", "OK");
            return;
        }
        var start = StartDatePicker.Date;
        var end = EndDatePicker.Date;
        if (end < start)
        {
            await DisplayAlertAsync("Błąd", "Data zakończenia nie może być wcześniejsza niż rozpoczęcia.", "OK");
            return;
        }
        try
        {
            if (_id.HasValue)
            {
                var res = await _api.PutAsJsonAsync($"/api/v1/periods/{_id.Value}",
                    new { Label = label, StartDate = start, EndDate = end });
                if (!res.IsSuccessStatusCode)
                {
                    await DisplayAlertAsync("Błąd", "Nie udało się zapisać.", "OK");
                    return;
                }
            }
            else
            {
                var res = await _api.PostAsJsonAsync("/api/v1/periods",
                    new { Label = label, StartDate = start, EndDate = end });
                if (!res.IsSuccessStatusCode)
                {
                    await DisplayAlertAsync("Błąd", "Nie udało się dodać okresu.", "OK");
                    return;
                }
            }
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
        }
    }

    public class PeriodDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
