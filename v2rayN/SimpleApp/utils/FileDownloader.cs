using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class FileDownloader
{
    private readonly string _url;
    private readonly string _savePath;
    private readonly int _threadCount;
    private readonly HttpClient _httpClient;
    private long _fileSize;
    private long _downloadedSize;
    private readonly object _lock = new object();

    public FileDownloader(string url, string savePath, int threadCount = 4)
    {
        _url = url;
        _savePath = savePath;
        _threadCount = threadCount;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task DownloadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取文件大小
            _fileSize = await GetFileSizeAsync();
            if (_fileSize == 0)
                throw new Exception("无法获取文件大小");

            // 计算每个线程的下载范围
            long chunkSize = _fileSize / _threadCount;
            Task[] downloadTasks = new Task[_threadCount];

            // 创建临时文件
            using (var fs = new FileStream(_savePath, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(_fileSize);
            }

            // 启动下载线程
            for (int i = 0; i < _threadCount; i++)
            {
                long start = i * chunkSize;
                long end = (i == _threadCount - 1) ? _fileSize - 1 : start + chunkSize - 1;
                downloadTasks[i] = DownloadPartAsync(start, end, cancellationToken);
            }

            // 等待所有下载线程完成
            await Task.WhenAll(downloadTasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载失败: {ex.Message}");
            throw;
        }
    }

    private async Task<long> GetFileSizeAsync()
    {
        var response = await _httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Head, _url));
        response.EnsureSuccessStatusCode();
        return response.Content.Headers.ContentLength ?? 0;
    }

    private async Task DownloadPartAsync(long start, long end, CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(_savePath, FileMode.Open, FileAccess.Write, FileShare.Write))
                {
                    fileStream.Position = start;
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                        lock (_lock)
                        {
                            _downloadedSize += bytesRead;
                            double progress = (double)_downloadedSize / _fileSize * 100;
                            Console.WriteLine($"下载进度: {progress:F2}%");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"分片下载失败: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// 使用示例
//public class Program
//{
//    public static async Task Main()
//    {
//        string url = "http://example.com/largefile.zip";
//        string savePath = "largefile.zip";

//        using var downloader = new FileDownloader(url, savePath, threadCount: 4);
//        try
//        {
//            await downloader.DownloadAsync();
//            Console.WriteLine("下载完成!");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"下载失败: {ex.Message}");
//        }
//    }
//}
