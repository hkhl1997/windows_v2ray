using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Controls;
using System.Net.Http;
using System.Threading;
using Serilog;
using System.Text.Json;
using System.Security.Policy;
using System.Windows.Markup;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Net;
using System.IO;
using ServiceLib.Handler.SysProxy;
using ServiceLib.Enums;
using ServiceLib.Handler;
using ServiceLib.Models;
using ServiceLib.Common;
using ServiceLib.ViewModels;
using SimpleApp.utils;
using System.Buffers.Text;
using ServiceLib.Resx;

namespace SimpleApp
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CSharp
    {
        private static string _code;
        public static CSharp Instance = new CSharp();
        private static string SALT = "un(jgkd!%#$ure)^";

        private static readonly string cookieFilePath = "cookies.json";
        private static readonly PersistentCookieManager cookieManager = new PersistentCookieManager(cookieFilePath);

        private CSharp() {

        }


        private async void ExcuteJavaScript(string js)
        {
            Log.Information($"ExcuteJavaScript {js}");

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync(js);
            });
        }

        public async void showMessage(string message)
        {
            await Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync($"alert('hahaha')");
        }

        private static string GetSid()
        {
            string machineId = MachineIdentifier.GetUniqueMachineId();
            machineId = CalculateMD5(machineId);
            string sid = machineId + '/' + CalculateMD5(machineId + SALT);
            return sid;
        }

        public static string CalculateMD5(string input)
        {
            // 创建 MD5 实例
            using (MD5 md5 = MD5.Create())
            {
                // 将输入字符串转换为字节数组并计算哈希值
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // 将哈希字节数组转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 模拟原 Java 代码中 dData 方法：对传入数据进行解密，并通过回调返回结果。
        /// 参数 javascriptCallback 为回调函数，在实际中你可以调用 WebView 的相应方法。
        /// </summary>
        /// <param name="code">code 字符串</param>
        /// <param name="d">加密的 Base64 字符串</param>
        /// <param name="cb">JavaScript 回调函数名称（此处仅作拼接字符串示例）</param>
        /// <param name="javascriptCallback">处理回调的委托，例如调用 WebView 的 EvaluateJavascript</param>
        public void dData(string code, string d, string cb)
        {
            _code = code;
            // 拼接 preShare 字符串
            string preShare = "C1A8P117-8KC5-4324-9B15-5EC6E44BD0B0" + code;
            try
            {
                // 调用解密方法
                string res = CryptoHelper.Decrypt(d, preShare);

                string jsCall = "javascript:" + cb + "(`" + res + "`)";

                ExcuteJavaScript(jsCall);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async void start(string json)
        {
            Log.Information("Start...............");
            //string url = "https://mini.aenemos.com/update_files.zip";
            //string savePath = "update_files.zip";

            //var downloader = new FileDownloader(url, savePath, threadCount: 4);
            //try
            //{
            //    await downloader.DownloadAsync();
            //    Console.WriteLine("下载完成!");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"下载失败: {ex.Message}");
            //}

            //var processor = new FileProcessor(savePath);

            //try
            //{
            //    await processor.ProcessAsync();
            //    Console.WriteLine("升级完成!");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"升级失败: {ex.Message}");
            //}
            //return;


            try
            {

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Line line = JsonSerializer.Deserialize<Line>(json, options);

                if (line == null)
                {
                    Log.Error("线路为空");
                    return;
                }

                //string preShare = "C1A8C117-8CA5-4124-9A15-5EC6D44DD0B0" + line.Code;

                //string ss = CryptoHelper.Decrypt(line.Domain, preShare);

                var node = new ProfileItem();

                node.Address = "abac.aaa.com";
                node.ConfigType = EConfigType.Shadowsocks;
                node.ConfigVersion = 2;
                node.DisplayLog = true;

                Log.Information($"Start...............line.Type{line.Type}     line.Code {line.Code}     line.Id{line.Id.ToString()}      line.EndTime{line.EndTime}      line.Mode{line.Mode.ToString()}      line.BackMainland{line.BackMainland.ToString()}    json {json}");

                node.Id = GenerateEncodedDomainConfig(line.Type, line.Code, line.Id.ToString(), line.EndTime, line.Mode.ToString(), line.BackMainland.ToString(), json);
                node.IndexId = "1";
                node.IsSub = true;
                node.Port = 3388;
                node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
                node.Security = "aes-256-gcm";
                node.HeaderType = "http";
                node.Network = "tcp";
                node.Subid = "2";


                


                //if (ss.Contains("obfs-local"))
                //{
                //    var config = SSParser.ParseSS_Http(ss);

                //    node.Address = config.Host;
                //    node.ConfigType = EConfigType.Shadowsocks;
                //    node.ConfigVersion = 2;
                //    node.DisplayLog = true;
                //    node.Id = config.Password;
                //    node.IndexId = "1";
                //    node.IsSub = true;
                //    node.Port = Convert.ToInt32(config.Port);
                //    node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
                //    node.Security = config.Method;
                //    node.HeaderType = "http";
                //    node.RequestHost = config.ObfsHost;
                //    node.Network = "tcp";
                //    node.Subid = "2";
                //}
                //else
                //{
                //    string[] parts = ss.Split('#');

                //    byte[] bytes = Convert.FromBase64String(parts[0]);

                //    string base64 = Encoding.UTF8.GetString(bytes);

                //    SSConfig config = SSConfig.ParseServerConfig(base64);

                //    node.Address = config.Domain;
                //    node.ConfigType = EConfigType.Shadowsocks;
                //    node.ConfigVersion = 2;
                //    node.DisplayLog = true;
                //    node.Id = config.Password;
                //    node.IndexId = "1";
                //    node.IsSub = true;
                //    node.Port = config.Port;
                //    node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
                //    node.Security = config.EncryptionMethod;
                //    node.Subid = "2";
                //}

                if (line.Tun == 1)
                {
                    await Global.SingleTon.ViewModel.DoEnableTun(true);
                }
                else
                {
                    await Global.SingleTon.ViewModel.DoEnableTun(false);
                }

                //if (line.Mode == 0)
                //{
                //    await Global.SingleTon.ViewModel.ChangeRouteModelAsync(true);
                //}
                //else
                //{
                //    await Global.SingleTon.ViewModel.ChangeRouteModelAsync(false);
                //}

                Global.SingleTon.ViewModel.CurrentNode = node;
                await Global.SingleTon.ViewModel.Reload();

                //if (Global.SingleTon.Models == Models.PAC)
                //{
                //    ProxySettingWindows.SetProxy($"https://zlj.cxnbgj.com/get/{line.Code}.pac", "", 4);

                //    ExcuteJavaScript($"displayConnected('{Global.SingleTon.SessionToken}')");

                //    return;
                //}

                //var port = AppHandler.Instance.GetLocalPort(EInboundProtocol.socks);

                //await SetWindowsProxyPac(port);

                //var node = new ProfileItem();

                //node.Address = "53bdde59.flzxjxo53slwj3n.e7mbqhx.com";
                //node.ConfigType = EConfigType.Shadowsocks;
                //node.ConfigVersion = 2;
                //node.DisplayLog = true;
                //node.Id = "OwzVzPeKuRZm";
                //node.IndexId = "1";
                //node.IsSub = true;
                //node.Port = 57654;
                //node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
                //node.Security = "chacha20-ietf-poly1305";
                //node.Subid = "2";

                //var task = Task.Run(async () =>
                //{
                //    await CoreHandler.Instance.LoadCore(node);

                //    //var node = await ConfigHandler.GetDefaultServer(AppHandler.Instance.Config);
                //    //await CoreHandler.Instance.LoadCore(node);

                //    //await SysProxyHandler.UpdateSysProxy(AppHandler.Instance.Config, false);


                //});

                ExcuteJavaScript($"displayConnected('{Global.SingleTon.SessionToken}')");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public static string GenerateEncodedDomainConfig(
            string connectionType,
            string code,
            string id,
            string expire,
            string mode,
            string backMainland,
            string jsonString)
        {
            try
            {
                // Build primary domain configuration
                var primaryDomain = JsonSerializer.Deserialize<JsonElement>(jsonString)
                    .GetProperty("domain").GetString()?.Trim()
                    ?? throw new InvalidOperationException("Domain is missing in JSON");

                var domainBuilder = new StringBuilder()
                    .Append(connectionType).Append('#')
                    .Append(code).Append('#')
                    .Append(id).Append('#')
                    .Append("windows").Append('#')
                    .Append("2.1").Append('#')
                    .Append(expire).Append('#')
                    .Append(mode).Append('#')
                    .Append(backMainland).Append('#')
                    .Append(primaryDomain);

                // Process backlines if present
                if (jsonString.ToLower().Contains("\"backlines\""))
                {
                    var backlines = JsonSerializer.Deserialize<JsonElement>(jsonString)
                        .GetProperty("backlines").EnumerateArray();

                    foreach (var item in backlines)
                    {
                        int lineId = item.GetProperty("line_id").GetInt32();
                        string type = item.GetProperty("type").GetString()
                            ?? throw new InvalidOperationException("Type is missing");
                        string domain = item.GetProperty("domain").GetString()?.Trim()
                            ?? throw new InvalidOperationException("Domain is missing");

                        domainBuilder.Append('\t')
                            .Append(connectionType).Append('#')
                            .Append(code).Append('#')
                            .Append(lineId).Append('#')
                            .Append("windows").Append('#')
                            .Append("2.1").Append('#')
                            .Append(expire).Append('#')
                            .Append(mode).Append('#')
                            .Append(backMainland).Append('#')
                            .Append(domain);
                    }
                }

                // Encode to Base64
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(domainBuilder.ToString()));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate domain configuration", ex);
            }
        }

        private static async Task SetWindowsProxyPac(int port)
        {
            var portPac = AppHandler.Instance.GetLocalPort(EInboundProtocol.pac);
            await PacHandler.Start(Utils.GetConfigPath(), port, portPac);
            var strProxy = $"{Global.HttpProtocol}{Global.Loopback}:{portPac}/pac?t={DateTime.Now.Ticks}";
            ProxySettingWindows.SetProxy(strProxy, "", 4);
        }

        public void stop() {

            Log.Information("stop...");

            try
            {
                _ = ProxySettingWindows.UnsetProxy();

                ExcuteJavaScript($"breakConnection()");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public async void getStatus() {

            try
            {
                string method = $"setAppVersion('2.0', 'windows','','wm004')";

                await Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync(method);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public void setSpeedMode(int speedMode) { 
            Debug.WriteLine(speedMode.ToString());

            //if (speedMode == 0) {
            //    Global.SingleTon.ViewModel.ChangeRouteModelAsync(true);
            //}
            //else
            //{
            //    Global.SingleTon.ViewModel.ChangeRouteModelAsync(false);
            //}
        }

        public async void httpGetAsync(string url, string success, string error)
        {
            Log.Information($"httpGetAsync({url}, {success}, {error}");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string guid = Guid.NewGuid().ToString();
                    // 设置请求的超时时间
                    client.Timeout = TimeSpan.FromSeconds(5);

                    client.DefaultRequestHeaders.Add("accept-source", GetSid());

                    string cookies = LoadCookies();

                    if (!string.IsNullOrEmpty(cookies))
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookies);
                    }

                    // 发送 GET 请求
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.Headers.Contains("Set-Cookie"))
                    {
                        // 获取单个 header 的值（返回一个集合，可以有多个值）
                        var headerValue = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                        Log.Error($"Header Value: {headerValue}");

                        SaveCookies(headerValue);
                    }

                    foreach (var header in response.Headers)
                    {
                        Log.Error($"{header.Key}: {string.Join(", ", header.Value)}");
                    }

                    // 确保响应成功
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    string content = await response.Content.ReadAsStringAsync();

                    string method = $"{success}('{content}')";

                    Log.Error($"httpGetAsync result {content}. excute : {method}");

                    await Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync(method);
                }
                catch (HttpRequestException e)
                {
                    Log.Error(e.ToString());
                    // 捕获请求异常
                    Log.Information($"Request error: {e.Message}");
                    HandleError(error, e.Message);
                }
                catch (TaskCanceledException e)
                {
                    Log.Error(e.ToString());
                    // 处理超时异常
                    if (e.CancellationToken.IsCancellationRequested)
                    {
                        Log.Information("Request was canceled.");
                        HandleError(error, "Request was canceled.");
                    }
                    else
                    {
                        Log.Information("Request timed out.");
                        HandleError(error, "Request timed out.");
                    }
                }
            }
        }

        public async void httpPostAsync(string url, string data, string success, string error)
        {
            Log.Information($"httpPostAsync({url}, {success}, {error}");
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string guid = Guid.NewGuid().ToString();
                    // 设置请求的超时时间
                    client.Timeout = TimeSpan.FromSeconds(5);

                    client.DefaultRequestHeaders.Add("accept-source", GetSid());

                    string cookies = LoadCookies();

                    if (!string.IsNullOrEmpty(cookies))
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookies);
                    }

                    Log.Error("httpPostAsync Print Cookies before post end.");

                    // 创建请求内容并设置 Content-Type 为 application/json
                    HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");

                    // 发送 POST 请求
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    foreach (var header in response.Headers)
                    {
                        Log.Error($"{header.Key}: {string.Join(", ", header.Value)}");
                    }

                    if (response.Headers.Contains("Set-Cookie"))
                    {
                        // 获取单个 header 的值（返回一个集合，可以有多个值）
                        var headerValue = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                        Log.Error($"Header Value: {headerValue}");

                        SaveCookies(headerValue);
                    }

                    // 确保响应成功
                    response.EnsureSuccessStatusCode();

                    Log.Error("httpPostAsync Print Cookies after post end.");

                    // 读取响应内容
                    string responseContent = await response.Content.ReadAsStringAsync();

                    Log.Error($"httpPostAsync result {responseContent}. excutte: {success}('{responseContent}')");

                    await Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync($"{success}('{responseContent}')");
                }
                catch (HttpRequestException e)
                {
                    Log.Error(e.ToString());
                    // 捕获请求异常
                    Log.Information($"Request error: {e.Message}");
                    HandleError(error, e.Message);
                }
                catch (TaskCanceledException e)
                {
                    Log.Error(e.ToString());
                    // 处理超时异常
                    if (e.CancellationToken.IsCancellationRequested)
                    {
                        Log.Information("Request was canceled.");
                        HandleError(error, "Request was canceled.");
                    }
                    else
                    {
                        Log.Information("Request timed out.");
                        HandleError(error, "Request timed out.");
                    }
                }
            }
        }

        private string LoadCookies()
        {
            if (!File.Exists(cookieFilePath))
                return string.Empty;

            var cookies = File.ReadAllText(cookieFilePath);

            return cookies;
        }

        private void SaveCookies(string cookies)
        {
            File.WriteAllText(cookieFilePath, cookies);
        }

        private void PrintCookies(CookieContainer cookieContainer,string guid)
        {
            foreach (Cookie cookie in cookieContainer.GetAllCookies()) // 设置一个合适的域名
            {
                var serializableCookie = new SerializableCookie(cookie);
                string key = GetCookieKey(serializableCookie);

                Log.Error($"     {guid}" + key);
            }
        }

        private string GetCookieKey(SerializableCookie cookie)
        {
            return $"{cookie.Name.ToLower()}|{cookie.Domain.ToLower()}|{cookie.Path}|{cookie.Value}|{cookie.Expires}|{cookie.HttpOnly}";
        }

        private async void HandleError(string errorMethod, string message) {
            await Global.SingleTon.WebView.CoreWebView2.ExecuteScriptAsync($"{errorMethod}('{message}')");
        }

        public void openUpdateUrl(string url) {
            OpenBrowser(url);
        }
        //displayConnected(guid)
        //breakConnection(error)
        //setAppVersion(version, device)
        //showMessage(error)

        public void OpenBrowser(string json)
        {
            try
            {
                // 使用方法1：通过类反序列化
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                UpdateInfo updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json, options);

                Process.Start(new ProcessStartInfo
                {
                    FileName = updateInfo.UpdateUrl,
                    UseShellExecute = true  // 使用默认浏览器打开URL
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                //MessageBox.Show("Error opening browser: " + ex.Message);
            }
        }

        public void exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    public class UpdateInfo
    {
        [JsonPropertyName("update_url")]
        public string UpdateUrl { get; set; }
    }
}
