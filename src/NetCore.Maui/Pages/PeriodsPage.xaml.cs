using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class PeriodsPage : ContentPage
{
    private readonly ApiClient _api;

    public PeriodsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MessageLabel.IsVisible = false;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var list = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
            List.ItemsSource = list ?? new List<PeriodDto>();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var page = new PeriodEditPage(_api, null);
        await Navigation.PushModalAsync(page);
        await LoadAsync();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not PeriodDto item) return;
        List.SelectedItem = null;
        var editPage = new PeriodEditPage(_api, new PeriodEditPage.PeriodDto
        {
            Id = item.Id,
            Label = item.Label,
            StartDate = item.StartDate,
            EndDate = item.EndDate
        });
        await Navigation.PushModalAsync(editPage);
        await LoadAsync();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not PeriodDto item) return;
        var ok = await DisplayAlertAsync("Usuń okres", "Czy na pewno usunąć \"" + item.Label + "\"?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/periods/{item.Id}");
            if (res.IsSuccessStatusCode)
                await LoadAsync();
            else
                await DisplayAlertAsync("Błąd", "Nie udało się usunąć.", "OK");
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
