using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class DepartmentsPage : ContentPage
{
    private readonly ApiClient _api;

    public DepartmentsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var list = await _api.GetFromJsonAsync<List<DeptDto>>("/api/v1/departments");
            List.ItemsSource = list ?? new List<DeptDto>();
        }
        catch
        {
            List.ItemsSource = new List<DeptDto>();
        }
    }

    private class DeptDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
}
