using Microsoft.Extensions.Logging;
using NetCore.Maui.Pages;
using NetCore.Maui.Services;

namespace NetCore.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var baseUrl = "https://localhost:5001";
		builder.Services.AddSingleton(_ => new HttpClient());
		builder.Services.AddSingleton(sp => new AuthService(sp.GetRequiredService<HttpClient>(), baseUrl));
		builder.Services.AddSingleton(sp => new ApiClient(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<AuthService>(), baseUrl));
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<ChannelsPage>();
		builder.Services.AddTransient<ReportsPage>();
		builder.Services.AddTransient<BonusesPage>();
		builder.Services.AddTransient<DepartmentsPage>();
		builder.Services.AddTransient<PeriodsPage>();
		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
