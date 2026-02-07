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
		var departments = serviceProvider.GetRequiredService<DepartmentsPage>();
		var periods = serviceProvider.GetRequiredService<PeriodsPage>();
		Items.Add(new TabBar
		{
			Items =
			{
				new ShellContent { Content = dashboard, Route = "Dashboard", Title = "Dashboard" },
				new ShellContent { Content = reports, Route = "Reports", Title = "Raporty" },
				new ShellContent { Content = channels, Route = "Channels", Title = "Kanały" },
				new ShellContent { Content = departments, Route = "Departments", Title = "Działy" },
				new ShellContent { Content = periods, Route = "Periods", Title = "Okresy" },
				new ShellContent { Content = bonuses, Route = "Bonuses", Title = "Premie" }
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
