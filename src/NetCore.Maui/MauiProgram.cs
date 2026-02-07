using System.Net.Http;
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

		// API w tym projekcie domyślnie nasłuchuje na http://localhost:5174 (profil „http”). Jeśli uruchomisz z --launch-profile https, zmień na https://localhost:7031.
		var baseUrl = "http://localhost:5174";
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
