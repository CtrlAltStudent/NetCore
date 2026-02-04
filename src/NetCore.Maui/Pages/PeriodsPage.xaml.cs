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
        try
        {
            var list = await _api.GetFromJsonAsync<List<PeriodDto>>("/api/v1/periods");
            List.ItemsSource = list ?? new List<PeriodDto>();
        }
        catch
        {
            List.ItemsSource = new List<PeriodDto>();
        }
    }

    private class PeriodDto { public Guid Id { get; set; } public string Label { get; set; } = ""; }
}
