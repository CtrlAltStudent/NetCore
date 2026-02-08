using Microsoft.Extensions.DependencyInjection;
using NetCore.Maui.Pages;
using NetCore.Maui.Services;

namespace NetCore.Maui;

public partial class AppShell : Shell
{
	private readonly IServiceProvider _serviceProvider;
	private readonly AuthService _auth;

	public AppShell(IServiceProvider serviceProvider, AuthService auth)
	{
		_serviceProvider = serviceProvider;
		_auth = auth;
		InitializeComponent();
		var dashboard = serviceProvider.GetRequiredService<DashboardPage>();
		var channels = serviceProvider.GetRequiredService<ChannelsPage>();
		var reports = serviceProvider.GetRequiredService<ReportsPage>();
		var bonuses = serviceProvider.GetRequiredService<BonusesPage>();
		var bonusRules = serviceProvider.GetRequiredService<BonusRulesPage>();
		var departments = serviceProvider.GetRequiredService<DepartmentsPage>();
		var employees = serviceProvider.GetRequiredService<EmployeesPage>();
		var periods = serviceProvider.GetRequiredService<PeriodsPage>();
		var revenues = serviceProvider.GetRequiredService<RevenuesPage>();
		var costs = serviceProvider.GetRequiredService<CostsPage>();
		var settings = serviceProvider.GetRequiredService<SettingsPage>();
		Items.Add(new TabBar
		{
			Items =
			{
				new ShellContent { Content = dashboard, Route = "Dashboard", Title = "Dashboard" },
				new ShellContent { Content = reports, Route = "Reports", Title = "Raporty" },
				new ShellContent { Content = channels, Route = "Channels", Title = "Kanały" },
				new ShellContent { Content = departments, Route = "Departments", Title = "Działy" },
				new ShellContent { Content = employees, Route = "Employees", Title = "Pracownicy" },
				new ShellContent { Content = periods, Route = "Periods", Title = "Okresy" },
				new ShellContent { Content = revenues, Route = "Revenues", Title = "Przychody" },
				new ShellContent { Content = costs, Route = "Costs", Title = "Koszty" },
				new ShellContent { Content = bonuses, Route = "Bonuses", Title = "Premie" },
				new ShellContent { Content = bonusRules, Route = "BonusRules", Title = "Reguły premii" },
				new ShellContent { Content = settings, Route = "Settings", Title = "Ustawienia" }
			}
		});
	}

	private async void OnLogoutClicked(object? sender, EventArgs e)
	{
		await _auth.LogoutAsync();
		if (Application.Current?.Windows.Count > 0)
			Application.Current.Windows[0].Page = new LoginPage(_auth);
	}
}
