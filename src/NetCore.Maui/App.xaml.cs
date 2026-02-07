namespace NetCore.Maui;

public partial class App : Application
{
	/// <summary>Kontener DI – dostępny od startu (np. na Windows Handler strony może być jeszcze null).</summary>
	public static IServiceProvider? Services { get; private set; }

	public App(IServiceProvider services)
	{
		Services = services;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new Pages.BootstrapPage());
	}
}