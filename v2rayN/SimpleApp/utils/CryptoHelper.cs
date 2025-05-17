using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.utils;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class CryptoHelper
{
    /// <summary>
    /// 根据输入字符串计算 MD5 哈希，返回字节数组
    /// </summary>
    public static byte[] GetMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            return md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
    }

    /// <summary>
    /// 根据输入字符串计算 MD5 哈希，并以 32 位十六进制字符串返回
    /// </summary>
    public static string GetMD5Hex(string input)
    {
        byte[] hashBytes = GetMD5(input);
        StringBuilder hexString = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            hexString.Append(b.ToString("x2"));
        }
        return hexString.ToString();
    }

    /// <summary>
    /// 将十六进制字符串转换为字节数组。
    /// 注意：此方法按照每个字符转换为一个字节，与原 Java 代码逻辑保持一致。
    /// 若需求为每两个字符转换为一个字节，则需调整循环步长为 2。
    /// </summary>
    public static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] byteArray = new byte[length];
        for (int i = 0; i < length; i++)
        {
            byteArray[i] = Convert.ToByte(hex.Substring(i, 1), 16);
        }
        return byteArray;
    }

    /// <summary>
    /// 使用 AES-128/CBC/PKCS7Padding 模式解密数据。
    /// 密钥与 IV 均通过对 secret 计算 MD5 后分割得到（前 16 字符作为 key，后 16 字符作为 iv）。
    /// </summary>
    public static string Decrypt(string encryptedText, string secret)
    {
        // 获取 secret 的 MD5 十六进制字符串（32 个字符）
        string md5Hex = GetMD5Hex(secret);
        // 前 16 个字符作为密钥
        string kStr = md5Hex.Substring(0, 16);
        // 后 16 个字符作为 IV
        string vStr = md5Hex.Substring(16, 16);

        // 将密钥和 IV 转换为字节数组（使用 UTF8 编码，与 Java 实现一致）
        byte[] key = Encoding.UTF8.GetBytes(kStr);
        byte[] iv = Encoding.UTF8.GetBytes(vStr);

        // 从 Base64 解码加密文本
        byte[] decodedEncryptedText = Convert.FromBase64String(encryptedText);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            aesAlg.Mode = System.Security.Cryptography.CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // 创建解密器并执行解密
            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            {
                byte[] decryptedBytes = decryptor.TransformFinalBlock(decodedEncryptedText, 0, decodedEncryptedText.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}

