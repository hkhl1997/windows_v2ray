using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using Serilog;
using ServiceLib.Handler;
using ServiceLib.Handler.SysProxy;
using SimpleApp.utils;
using Application = System.Windows.Application;

namespace SimpleApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const string UniqueEventName = "Yunbao_UniqueEventName";

    private EventWaitHandle eventWaitHandle;
    private static Mutex mutex;
    protected override void OnStartup(StartupEventArgs e)
    {
        string mutexName = "Global\\yunbao";
        bool isNewInstance;
        mutex = new Mutex(true, mutexName, out isNewInstance);

        if (isNewInstance)
        {
            // 第一个实例
            // 创建 EventWaitHandle，用于 IPC
            eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // 开始监听来自其他实例的信号
            ListenForActivationSignal();


            base.OnStartup(e);


            try
            {
                _ = ProxySettingWindows.UnsetProxy();

                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console() // 可选：输出到控制台
                .WriteTo.File(Path.Combine("logs/log.txt"), rollingInterval: RollingInterval.Day) // 日志输出到文件，按天创建新文件
                .CreateLogger();

                //Task.Run(() => {
                //    const string updateUrl = "https://d.ybjsq.net/test/h5.zip";
                //    UpdateManager.Run(updateUrl);
                //});
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
        else
        {
            // 已有实例在运行
            try
            {
                // 打开已存在的 EventWaitHandle 并发送信号
                eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                eventWaitHandle.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // 处理异常（可能是已有实例尚未创建事件）
            }

            // 关闭新实例
            Shutdown();
        }
    }

    private void ListenForActivationSignal()
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            while (eventWaitHandle.WaitOne())
            {
                // 收到信号，恢复并激活主窗口
                Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = Current.MainWindow;
                    if (mainWindow != null)
                    {
                        if (mainWindow.WindowState == WindowState.Minimized)
                        {
                            mainWindow.WindowState = WindowState.Normal;
                        }
                        mainWindow.Show();
                        mainWindow.Activate();
                    }
                });
            }
        });
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

