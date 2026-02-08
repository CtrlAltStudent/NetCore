using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class EmployeesPage : ContentPage
{
    private readonly ApiClient _api;
    private List<DeptDto> _departments = new();

    public EmployeesPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        MessageLabel.IsVisible = false;
        await LoadDepartmentsAsync();
        await LoadAsync();
    }

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            var list = await _api.GetFromJsonAsync<List<DeptDto>>("/api/v1/departments");
            _departments = list ?? new List<DeptDto>();
        }
        catch { _departments = new(); }
    }

    private async Task LoadAsync()
    {
        try
        {
            var list = await _api.GetFromJsonAsync<List<EmployeeDto>>("/api/v1/employees");
            var items = (list ?? new List<EmployeeDto>()).Select(e => new EmployeeItemVm
            {
                Id = e.Id,
                Name = e.Name,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.DepartmentId.HasValue
                    ? _departments.FirstOrDefault(d => d.Id == e.DepartmentId)?.Name ?? "(dział)"
                    : "—"
            }).ToList();
            List.ItemsSource = items;
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync("Nowy pracownik", "Imię i nazwisko:", "Zapisz", "Anuluj", maxLength: 200);
        if (string.IsNullOrWhiteSpace(name)) return;
        var departmentId = await PickDepartmentAsync();
        try
        {
            var res = await _api.PostAsJsonAsync("/api/v1/employees", new { Name = name.Trim(), DepartmentId = departmentId });
            if (res.IsSuccessStatusCode)
                await LoadAsync();
            else
                await DisplayAlertAsync("Błąd", "Nie udało się dodać pracownika.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not EmployeeItemVm item) return;
        List.SelectedItem = null;
        var name = await DisplayPromptAsync("Edytuj pracownika", "Imię i nazwisko:", "Zapisz", "Anuluj", null, 200, Keyboard.Default, item.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        var departmentId = await PickDepartmentAsync(item.DepartmentId);
        try
        {
            var res = await _api.PutAsJsonAsync($"/api/v1/employees/{item.Id}", new { Name = name.Trim(), DepartmentId = departmentId });
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

    private async Task<Guid?> PickDepartmentAsync(Guid? selected = null)
    {
        var options = _departments.Select(d => d.Name).ToList();
        if (options.Count == 0) return null;
        options.Add("(Brak)");
        var choice = await DisplayActionSheetAsync("Dział", "Anuluj", null, options.ToArray());
        if (choice == "Anuluj" || string.IsNullOrEmpty(choice)) return selected;
        if (choice == "(Brak)") return null;
        var idx = options.IndexOf(choice);
        if (idx < 0 || idx >= _departments.Count) return null;
        return _departments[idx].Id;
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not EmployeeItemVm item) return;
        var ok = await DisplayAlertAsync("Usuń pracownika", "Czy na pewno usunąć \"" + item.Name + "\"?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/employees/{item.Id}");
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

    private class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Guid? DepartmentId { get; set; }
    }

    private class DeptDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class EmployeeItemVm
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
    }
}
