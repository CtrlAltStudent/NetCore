using Microsoft.Maui.LifecycleEvents;

namespace NetCore.Maui.WinUI;

/// <summary>
/// Wyłącza systemowy backdrop i ustawia białe tło okna – bez tego na Windows widać tylko czarny ekran.
/// </summary>
public static class WindowsLifecycleExtensions
{
	public static void ConfigureWindowsLifecycle(this MauiAppBuilder builder)
	{
		builder.ConfigureLifecycleEvents(events =>
		{
			events.AddWindows(windows =>
			{
				windows.OnWindowCreated(window =>
				{
					if (window is not Microsoft.UI.Xaml.Window winUiWindow) return;
					winUiWindow.SystemBackdrop = null;
					SetWindowBackgroundWhite(winUiWindow);
				});
			});
		});
	}

	private static void SetWindowBackgroundWhite(Microsoft.UI.Xaml.Window w)
	{
		var white = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
		if (w.Content is Microsoft.UI.Xaml.Controls.Panel panel)
			panel.Background = white;
		else if (w.Content is Microsoft.UI.Xaml.Controls.Control control)
			control.Background = white;
	}
}
