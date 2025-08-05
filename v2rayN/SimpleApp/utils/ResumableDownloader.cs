using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.utils;
public sealed class ResumableDownloader
{
    public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;
    public event EventHandler<DownloadProgressEventArgs>? ProgressChanged;

    private readonly string _url;
    private readonly string _destPath;
    private readonly int _bufferSize;
    private Thread? _worker;

    public ResumableDownloader(string url, string destPath, int bufferSize = 81920)
    {
        _url = url;
        _destPath = destPath;
        _bufferSize = bufferSize;
    }

    public void Start()
    {
        _worker = new Thread(DownloadCore) { IsBackground = true };
        _worker.Start();
    }

    // ────────────────────────────────────────────────────────────────
    // 私有实现
    // ────────────────────────────────────────────────────────────────
    private async void DownloadCore()
    {
        Exception? failure = null;

        try
        {
            long existing = File.Exists(_destPath) ? new FileInfo(_destPath).Length : 0;

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            using var req = new HttpRequestMessage(HttpMethod.Get, _url);

            if (existing > 0)
                req.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existing, null);

            using var resp = await http.SendAsync(req,
                               HttpCompletionOption.ResponseHeadersRead,
                               CancellationToken.None);

            if (resp.StatusCode != HttpStatusCode.OK &&
                resp.StatusCode != HttpStatusCode.PartialContent)
                throw new InvalidOperationException($"HTTP {resp.StatusCode}");

            long? total = resp.Content.Headers.ContentLength;
            if (resp.StatusCode == HttpStatusCode.PartialContent && total.HasValue)
                total += existing;   // 服务器只返回剩余字节数，要加上已下载部分

            // 以 Append 模式写入
            await using var fs = new FileStream(_destPath, FileMode.Append,
                                                FileAccess.Write, FileShare.None,
                                                _bufferSize, useAsync: true);
            await using var stream = await resp.Content.ReadAsStreamAsync();

            var buffer = new byte[_bufferSize];
            long written = existing;

            int read;
            while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, read));
                written += read;
                ProgressChanged?.Invoke(this,
                    new DownloadProgressEventArgs(written, total));
            }
        }
        catch (Exception ex)
        {
            failure = ex;
        }
        finally
        {
            DownloadCompleted?.Invoke(this,
                new DownloadCompletedEventArgs(_destPath, failure));
        }
    }
}

// ───────────────────── 辅助事件数据类型 ─────────────────────
public sealed class DownloadCompletedEventArgs : EventArgs
{
    public string FilePath { get; }
    public Exception? Error { get; }

    public bool IsSuccess => Error is null;
    public DownloadCompletedEventArgs(string filePath, Exception? error)
    { FilePath = filePath; Error = error; }
}

public sealed class DownloadProgressEventArgs : EventArgs
{
    public long BytesReceived { get; }
    public long? TotalBytes { get; }

    public double? Percent => TotalBytes.HasValue
        ? BytesReceived * 100.0 / TotalBytes.Value
        : null;

    public DownloadProgressEventArgs(long received, long? total)
    { BytesReceived = received; TotalBytes = total; }
}
