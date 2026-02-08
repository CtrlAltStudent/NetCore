using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class BonusRulesPage : ContentPage
{
    private readonly ApiClient _api;
    private List<DeptDto> _departments = new();
    private static readonly string[] FormulaTypeNames = { "Procent z zysku działu", "Procent z zysku firmy", "Stała gdy cel" };

    public BonusRulesPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
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
        MessageLabel.IsVisible = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        try
        {
            var list = await _api.GetFromJsonAsync<List<BonusRuleDto>>("/api/v1/bonus-rules");
            var items = (list ?? new List<BonusRuleDto>()).Select(r => new BonusRuleItemVm
            {
                Id = r.Id,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.DepartmentId.HasValue
                    ? _departments.FirstOrDefault(d => d.Id == r.DepartmentId)?.Name ?? "(dział)"
                    : "—",
                FormulaType = r.FormulaType,
                FormulaTypeName = FormulaTypeNames[Math.Clamp(r.FormulaType, 0, 2)],
                ParametersJson = r.ParametersJson ?? "{}",
                ParametersPreview = (r.ParametersJson?.Length > 40 ? r.ParametersJson[..40] + "…" : r.ParametersJson) ?? "{}",
                IsActive = r.IsActive,
                IsActiveText = r.IsActive ? "Aktywna" : "Nieaktywna",
            }).ToList();
            List.ItemsSource = items;
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var departmentId = await PickDepartmentAsync();
        var formulaIndex = await PickFormulaTypeAsync(null);
        if (formulaIndex < 0) return;
        var parametersJson = await DisplayPromptAsync("Parametry (JSON)", "ParametersJson:", "Zapisz", "Anuluj", null, 500, Keyboard.Default, "{}");
        if (parametersJson == null) return;
        if (string.IsNullOrWhiteSpace(parametersJson)) parametersJson = "{}";
        var isActive = await DisplayAlertAsync("Reguła premii", "Czy reguła ma być aktywna?", "Tak", "Nie");
        try
        {
            var res = await _api.PostAsJsonAsync("/api/v1/bonus-rules", new
            {
                DepartmentId = departmentId,
                FormulaType = formulaIndex,
                ParametersJson = parametersJson,
                IsActive = isActive,
            });
            if (res.IsSuccessStatusCode)
                await LoadAsync();
            else
                await DisplayAlertAsync("Błąd", "Nie udało się dodać reguły.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Błąd", ex.Message, "OK");
        }
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection.FirstOrDefault() is not BonusRuleItemVm item) return;
        List.SelectedItem = null;
        var departmentId = await PickDepartmentAsync(item.DepartmentId);
        var formulaIndex = await PickFormulaTypeAsync(item.FormulaType);
        if (formulaIndex < 0) return;
        var parametersJson = await DisplayPromptAsync("Parametry (JSON)", "ParametersJson:", "Zapisz", "Anuluj", null, 500, Keyboard.Default, item.ParametersJson);
        if (parametersJson == null) return;
        if (string.IsNullOrWhiteSpace(parametersJson)) parametersJson = "{}";
        var isActive = await DisplayAlertAsync("Reguła premii", "Czy reguła ma być aktywna?", "Tak", "Nie");
        try
        {
            var res = await _api.PutAsJsonAsync($"/api/v1/bonus-rules/{item.Id}", new
            {
                DepartmentId = departmentId,
                FormulaType = formulaIndex,
                ParametersJson = parametersJson,
                IsActive = isActive,
            });
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

    private async Task<int> PickFormulaTypeAsync(int? current)
    {
        var choice = await DisplayActionSheetAsync("Typ formuły", "Anuluj", null, FormulaTypeNames);
        if (choice == "Anuluj" || string.IsNullOrEmpty(choice)) return current ?? -1;
        var idx = Array.IndexOf(FormulaTypeNames, choice);
        return idx >= 0 ? idx : (current ?? -1);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.BindingContext is not BonusRuleItemVm item) return;
        var ok = await DisplayAlertAsync("Usuń regułę", "Czy na pewno usunąć tę regułę?", "Usuń", "Anuluj");
        if (!ok) return;
        try
        {
            var res = await _api.DeleteAsync($"/api/v1/bonus-rules/{item.Id}");
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

    private class BonusRuleDto
    {
        public Guid Id { get; set; }
        public Guid? DepartmentId { get; set; }
        public int FormulaType { get; set; }
        public string? ParametersJson { get; set; }
        public bool IsActive { get; set; }
    }

    private class DeptDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class BonusRuleItemVm
    {
        public Guid Id { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public int FormulaType { get; set; }
        public string FormulaTypeName { get; set; } = "";
        public string ParametersJson { get; set; } = "";
        public string ParametersPreview { get; set; } = "";
        public bool IsActive { get; set; }
        public string IsActiveText { get; set; } = "";
    }
}
