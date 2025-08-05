using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp;
public class SSParser
{
    public class ParseResult
    {
        public string Method;
        public string Password;
        public string Host;
        public string Port;
        public string ObfsHost;
    }

    public static ParseResult ParseSS_Http(string base64Conf)
    {
        // 使用多个分隔符拆分
        char[] delimiters = new[] { '@', ':', '/', '?', '#' };
        string[] arrUrl = base64Conf.Split(delimiters);

        if (arrUrl.Length < 5)
        {
            return null;
        }

        string base64EncodedData = arrUrl[0];
        // 修复 base64 padding
        int mod = base64EncodedData.Length % 4;
        if (mod > 0)
        {
            base64EncodedData += new string('=', 4 - mod);
        }

        string method = null;
        string password = null;
        try
        {
            byte[] decodedBytes = Convert.FromBase64String(base64EncodedData);
            string decodedString = Encoding.UTF8.GetString(decodedBytes);
            var mp = decodedString.Split(':');
            if (mp.Length != 2)
            {
                return null;
            }
            method = mp[0];
            password = mp[1];
        }
        catch
        {
            return null;
        }

        string host = arrUrl[1];
        string port = arrUrl[2];
        string http = arrUrl[4];

        try
        {
            string httpDecoded = WebUtility.UrlDecode(http);
            char[] delim2 = new[] { '=', ':', '/', '?', '#', ';', '&' };
            string[] httpOpt = httpDecoded.Split(delim2, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < httpOpt.Length; i++)
            {
                if (httpOpt[i] == "obfs-host" && i + 1 < httpOpt.Length)
                {
                    return new ParseResult
                    {
                        Method = method,
                        Password = password,
                        Host = host,
                        Port = port,
                        ObfsHost = httpOpt[i + 1]
                    };
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
