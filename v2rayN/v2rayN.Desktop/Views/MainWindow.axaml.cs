using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using DialogHostAvalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Splat;
using v2rayN.Desktop.Common;
using v2rayN.Desktop.Handler;

namespace v2rayN.Desktop.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private static Config _config;
        private WindowNotificationManager? _manager;
        private CheckUpdateView? _checkUpdateView;
        private BackupAndRestoreView? _backupAndRestoreView;
        private bool _blCloseByUser = false;

        public MainWindow()
        {
            InitializeComponent();


            ViewModel = new MainWindowViewModel(UpdateViewHandler);




        }

        private async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
        {
            return await Task.FromResult(true);
        }

    
    }
}
