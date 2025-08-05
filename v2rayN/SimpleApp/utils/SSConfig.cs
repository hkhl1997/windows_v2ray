using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.utils;
public class SSConfig
{
    public string EncryptionMethod { get; set; }
    public string Password { get; set; }
    public string Domain { get; set; }
    public int Port { get; set; }

    public override string ToString()
    {
        return $"加密方式: {EncryptionMethod}, 密钥: {Password}, 域名: {Domain}, 端口: {Port}";
    }

    /// <summary>
    /// 解析格式形如 "chacha20-ietf-poly1305:bOFMFdRPLn5F@c9b20168.flbgpi-hk.p2tib8n.com:57602" 的字符串，
    /// 并返回一个包含加密方式、密钥、域名、端口的 ServerConfig 对象。
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <returns>解析后的 ServerConfig 对象</returns>
    public static SSConfig ParseServerConfig(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("输入字符串为空", nameof(input));

        // 按 '@' 分割字符串
        string[] parts = input.Split('@');
        if (parts.Length != 2)
            throw new FormatException("输入字符串格式不正确，缺少或多余 '@' 符号。");

        // 解析 '@' 前面的部分：加密方式与密钥（用冒号分隔）
        string[] preAtParts = parts[0].Split(':');
        if (preAtParts.Length != 2)
            throw new FormatException("输入字符串 '@' 前部分格式不正确，应为 '加密方式:密钥'。");
        string encryptionMethod = preAtParts[0];
        string password = preAtParts[1];

        // 解析 '@' 后面的部分：域名与端口（用冒号分隔）
        string[] postAtParts = parts[1].Split(':');
        if (postAtParts.Length != 2)
            throw new FormatException("输入字符串 '@' 后部分格式不正确，应为 '域名:端口'。");
        string domain = postAtParts[0];

        // 解析端口，转换为整数
        if (!int.TryParse(postAtParts[1], out int port))
            throw new FormatException("端口格式不正确，无法转换为整数。");

        // 构造并返回结果对象
        return new SSConfig
        {
            EncryptionMethod = encryptionMethod,
            Password = password,
            Domain = domain,
            Port = port
        };
    }
}
