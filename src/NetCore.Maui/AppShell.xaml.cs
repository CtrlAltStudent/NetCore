using Microsoft.Extensions.DependencyInjection;
using NetCore.Maui.Pages;

namespace NetCore.Maui;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider serviceProvider)
	{
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
}
