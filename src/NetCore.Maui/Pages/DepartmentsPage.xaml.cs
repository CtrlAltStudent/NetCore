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
        MessageLabel.IsVisible = false;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var list = await _api.GetFromJsonAsync<List<DeptDto>>("/api/v1/departments");
            List.ItemsSource = list ?? new List<DeptDto>();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nowy dział", "Nazwa działu:", "Zapisz", "Anuluj", maxLength: 200);
        if (string.IsNullOrWhiteSpace(name)) return;
        try
        {
            var res = await _api.PostAsJsonAsync("/api/v1/departments", new { Name = name.Trim() });
            if (res.IsSuccessStatusCode)
                await LoadAsync();
            else
                await DisplayAlertAsync("Błąd", "Nie udało się dodać działu.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not DeptDto item) return;
        List.SelectedItem = null;
        var name = await DisplayPromptAsync("Edytuj dział", "Nazwa:", "Zapisz", "Anuluj", null, 200, Keyboard.Default, item.Name);
        if (string.IsNullOrWhiteSpace(name) || name == item.Name) return;
        try
        {
            var res = await _api.PutAsJsonAsync($"/api/v1/departments/{item.Id}", new { Name = name.Trim() });
            if (res.IsSuccessStatusCode)
                await LoadAsync();
            else
                await DisplayAlertAsync("Błąd", "Nie udało się zapisać.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not DeptDto item) return;
        var ok = await DisplayAlertAsync("Usuń dział", "Czy na pewno usunąć \"" + item.Name + "\"?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/departments/{item.Id}");
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

    public class DeptDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
}
