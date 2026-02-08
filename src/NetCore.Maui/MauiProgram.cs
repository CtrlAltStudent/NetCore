using System.Net.Http;
using Microsoft.Extensions.Logging;
using NetCore.Maui.Pages;
using NetCore.Maui.Services;

#if WINDOWS
using NetCore.Maui.WinUI;
#endif

#if !WINDOWS
using LiveChartsCore.SkiaSharpView.Maui;
#endif

namespace NetCore.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if !WINDOWS
			.UseLiveCharts()
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if WINDOWS
		// Wyłącz gradientowe tło systemowe (Mica) – bez tego na Windows widać tylko tło, bez treści MAUI.
		builder.ConfigureWindowsLifecycle();
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Adres API można zmienić w Ustawieniach (Preferences). Domyślnie http://localhost:5174.
		builder.Services.AddSingleton<ApiBaseUrlService>();
		// Na Windows HttpClient domyślnie odrzuca certyfikat deweloperski localhost – w DEBUG akceptuj go.
		builder.Services.AddSingleton<HttpClient>(_ =>
		{
#if DEBUG
			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true; // akceptuj localhost dev cert
			return new HttpClient(handler);
#else
			return new HttpClient();
#endif
		});
		builder.Services.AddSingleton(sp => new AuthService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ApiBaseUrlService>()));
		builder.Services.AddSingleton(sp => new ApiClient(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<AuthService>(), sp.GetRequiredService<ApiBaseUrlService>()));
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<ChannelsPage>();
		builder.Services.AddTransient<ReportsPage>();
		builder.Services.AddTransient<BonusesPage>();
		builder.Services.AddTransient<BonusRulesPage>();
		builder.Services.AddTransient<DepartmentsPage>();
		builder.Services.AddTransient<EmployeesPage>();
		builder.Services.AddTransient<PeriodsPage>();
		builder.Services.AddTransient<RevenuesPage>();
		builder.Services.AddTransient<CostsPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}
}
