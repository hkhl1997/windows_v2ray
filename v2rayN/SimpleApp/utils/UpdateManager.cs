using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleApp.utils;
public static class UpdateManager
{
    // 更新入口：manifest、hash 等校验逻辑可在调用前做好
    public static void Run(string updateUrl)
    {
        // 1. 设定固定下载/解压根目录  …\InstallDir\UpdateFile
        string installDir = AppContext.BaseDirectory;
        string updateRoot = Path.Combine(installDir, "UpdateFile");
        string zipPath = Path.Combine(updateRoot, "update.zip");

        // 确保一个干净的 UpdateFile 目录（ResumableDownloader 需求）
        if (Directory.Exists(updateRoot))
            Directory.Delete(updateRoot, true);
        Directory.CreateDirectory(updateRoot);

        // 2. 创建断点续传下载器
        var dl = new ResumableDownloader(updateUrl, zipPath);

        // 3. 进度回调（可接到进度条）
        //dl.ProgressChanged += (_, e) =>
        //{
        //    if (e.Percent is { } p)
        //        Console.WriteLine($"下载进度：{p:F1}%");
        //    else
        //        Console.WriteLine($"已下载 {e.BytesReceived:N0} 字节");
        //};

        // 4. 完成回调：下载成功 → 调用 UpdateInstaller
        dl.DownloadCompleted += (_, e) =>
        {
            if (!e.IsSuccess)
            {
                //MessageBox.Show($"下载失败：{e.Error}", "更新", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // 解压并备份替换
                UpdateInstaller.Apply(e.FilePath, installDir);

                //MessageBox.Show(
                //    "更新已成功安装，重启应用后生效！",
                //    "更新",
                //    MessageBoxButton.OK,
                //    MessageBoxImage.Information);

                // 这里可选择自动重启
                // System.Windows.Forms.Application.Restart();  (WinForms)
                // or  Process.Start(Environment.ProcessPath!); Application.Current.Shutdown(); (WPF)
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"安装更新时出错：{ex}", "更新", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        // 5. 开始下载（后台线程）
        dl.Start();
    }
}
