using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp
{
    [Serializable]
    public class SerializableCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime Expires { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public bool Discard { get; set; }
        public string Comment { get; set; }
        public string CommentUri { get; set; }
        public string Port { get; set; }
        public string Version { get; set; }

        // Parameterless constructor for deserialization
        public SerializableCookie() { }

        // Convert Cookie to SerializableCookie
        public SerializableCookie(System.Net.Cookie cookie)
        {
            Name = cookie.Name;
            Value = cookie.Value;
            Domain = cookie.Domain;
            Path = cookie.Path;
            Expires = cookie.Expires;
            Secure = cookie.Secure;
            HttpOnly = cookie.HttpOnly;
            Discard = cookie.Discard;
            Comment = cookie.Comment;
            CommentUri = cookie.CommentUri?.ToString();
            Port = cookie.Port;
            Version = cookie.Version.ToString();
        }

        // Convert SerializableCookie back to Cookie
        public System.Net.Cookie ToCookie()
        {
            var cookie = new System.Net.Cookie(Name, Value, Path, Domain)
            {
                Expires = Expires,
                Secure = Secure,
                HttpOnly = HttpOnly,
                Discard = Discard,
                Comment = Comment,
                Port = Port,
                Version = int.TryParse(Version, out int ver) ? ver : 0
            };

            if (Uri.TryCreate(CommentUri, UriKind.Absolute, out Uri commentUri))
            {
                cookie.CommentUri = commentUri;
            }

            return cookie;
        }
    }
}
