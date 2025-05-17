using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleApp
{
    public class PersistentCookieManager
    {
        private Dictionary<string, SerializableCookie> cookieDictionary;
        private string cookieFilePath;

        public PersistentCookieManager(string filePath)
        {
            cookieDictionary = new Dictionary<string, SerializableCookie>();
            cookieFilePath = filePath;
        }

        private string GetCookieKey(SerializableCookie cookie)
        {
            return $"{cookie.Name.ToLower()}|{cookie.Domain.ToLower()}|{cookie.Path}";
        }

        /// <summary>
        /// 添加或更新一个 Cookie。
        /// </summary>
        public void AddOrUpdateCookie(Cookie newCookie)
        {
            if (newCookie == null)
                throw new ArgumentNullException(nameof(newCookie));

            var serializableCookie = new SerializableCookie(newCookie);
            string key = GetCookieKey(serializableCookie);

            if (cookieDictionary.ContainsKey(key))
            {
                // 更新现有 Cookie 的属性
                cookieDictionary[key] = serializableCookie;
            }
            else
            {
                // 添加新的 Cookie
                cookieDictionary[key] = serializableCookie;
            }
        }

        /// <summary>
        /// 获取所有 Cookies。
        /// </summary>
        public List<SerializableCookie> GetAllCookies()
        {
            return cookieDictionary.Values.ToList();
        }

        /// <summary>
        /// 序列化 Cookies 为 JSON 字符串并保存到文件。
        /// </summary>
        public void SaveCookies(CookieContainer cookieContainer)
        {
            var cookiesList = new List<SerializableCookie>();

            foreach (Cookie cookie in cookieContainer.GetAllCookies()) // 设置一个合适的域名
            {
                var serializableCookie = new SerializableCookie(cookie);
                string key = GetCookieKey(serializableCookie);

                if (cookieDictionary.ContainsKey(key))
                {
                    // 更新现有 Cookie
                    cookieDictionary[key] = serializableCookie;
                }
                else
                {
                    // 添加新的 Cookie
                    cookieDictionary[key] = serializableCookie;
                }
            }

            var uniqueCookies = cookieDictionary.Values.ToList();
            var json = JsonSerializer.Serialize(uniqueCookies, new JsonSerializerOptions { WriteIndented = true });

            Log.Information($"save cookies : {json}");

            File.WriteAllText(cookieFilePath, json);
        }

        /// <summary>
        /// 从文件反序列化 Cookies 并加载到 CookieContainer。
        /// </summary>
        public CookieContainer LoadCookies()
        {
            var cookieContainer = new CookieContainer();

            if (!File.Exists(cookieFilePath))
                return cookieContainer;

            var json = File.ReadAllText(cookieFilePath);

            Log.Information($"load cookies : {json}");

            var cookiesList = JsonSerializer.Deserialize<List<SerializableCookie>>(json);

            foreach (var cookie in cookiesList)
            {
                cookieContainer.Add(cookie.ToCookie());
            }

            return cookieContainer;
        }
    }
}
