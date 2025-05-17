using Microsoft.Web.WebView2.Core;
using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.Models;
using ServiceLib.ViewModels;
using Splat;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleApp;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        InitializeTrayIcon();

        Global.SingleTon.WebView = this.webView;

        MainWindowViewModel ViewModel = new MainWindowViewModel(UpdateViewHandler);

        Global.SingleTon.ViewModel = ViewModel;
    }

    private static async Task<bool> UpdateViewHandler(EViewAction action, object? obj)
    {
        return await Task.FromResult(true);
    }



    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        webView.EnsureCoreWebView2Async();
    }

    private async void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
        var environment = CoreWebView2Environment.CreateAsync();
        await webView.EnsureCoreWebView2Async(null);

        webView.CoreWebView2.AddHostObjectToScript("csharp", CSharp.Instance);

        await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
"var csharp = window.chrome.webview.hostObjects.csharp;");


        // 获取本地 HTML 文件路径
        string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "h5", "index.html");

        // 加载本地 HTML 文件
        webView.Source = new Uri(htmlFilePath);
    }

    private NotifyIcon _notifyIcon;
    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("main.ico"), // 使用自定义图标（确保路径正确）
            Visible = true,
            Text = "云宝加速" // 托盘鼠标悬停提示文本
        };

        // 创建右键菜单
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("打开", null, (s, e) => ShowWindow());
        contextMenu.Items.Add("退出", null, (s, e) => ExitApplication());

        _notifyIcon.ContextMenuStrip = contextMenu;

        // 托盘双击事件，双击打开应用窗口
        _notifyIcon.DoubleClick += (s, e) => ShowWindow();
    }

    private void ShowWindow()
    {
        Show();
        this.ShowInTaskbar = true;
        WindowState = WindowState.Normal;
        Activate(); // 窗口置于前台
    }

    private void ExitApplication()
    {
        this.WindowState = WindowState.Minimized;
        this.ShowInTaskbar = false;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    // 确保关闭应用时释放托盘图标
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
        this.ShowInTaskbar = false;
        //_notifyIcon.Visible = false;
        //_notifyIcon.Dispose();
        e.Cancel = true;
    }
}
