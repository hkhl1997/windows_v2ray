using Serilog;
using ServiceLib.Handler;
using ServiceLib.Handler.SysProxy;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;

namespace SimpleApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);


        try
        {
            _ = ProxySettingWindows.UnsetProxy();

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() // 可选：输出到控制台
            .WriteTo.File(Path.Combine("logs/log.txt"), rollingInterval: RollingInterval.Day) // 日志输出到文件，按天创建新文件
            .CreateLogger();
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }

        this.SessionEnding += App_SessionEnding;
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            try
            {
                _ = ProxySettingWindows.UnsetProxy();

                Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        };

        if (!AppHandler.Instance.InitApp())
        {
            Environment.Exit(0);
            return;
        }
    }

    private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        _ = ProxySettingWindows.UnsetProxy();
        // 弹出对话框询问用户是否取消关机/注销请求
        MessageBoxResult result = System.Windows.MessageBox.Show("关机前请正常退出云宝",
                                                  "云宝退出确认",
                                                  MessageBoxButton.OK,
                                                  MessageBoxImage.Question);
    }
}

