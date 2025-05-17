using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Splat;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.Views;

namespace v2rayN.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        if (!AppHandler.Instance.InitApp())
        {
            Environment.Exit(0);
            return;
        }
        //AvaloniaXamlLoader.Load(this);

        //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        //var ViewModel = new StatusBarViewModel(null);
        //Locator.CurrentMutable.RegisterLazySingleton(() => ViewModel, typeof(StatusBarViewModel));
        //this.DataContext = ViewModel;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            //    AppHandler.Instance.InitComponents();

            //    desktop.Exit += OnExit;
            desktop.MainWindow = new MainWindow();
        }

        //base.OnFrameworkInitializationCompleted();
    }
}
