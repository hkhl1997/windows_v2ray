using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleApp
{
    public class Line
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        public string FlagUrl { 
            get {
                return $"images/flags/{Country}.png";
            } 
        }

        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("userNo")]
        public int Sn { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("line_id")]
        public int LineId { get; set; }

        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }

        [JsonPropertyName("mode")]
        public int Mode { get; set; }

        [JsonPropertyName("tun")]
        public int Tun { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("endtime")]
        public string EndTime { get; set; }

        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("back_mainland")]
        public int BackMainland { get; set; }
        

        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
