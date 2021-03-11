using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMSuggestionBot.Core
{
    public class BotConfig
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("channel")]
        public ulong Channel { get; set; }
        [JsonPropertyName("prefix")]
        public string Prefix { get; set; }
        
        public BotConfig(string token, ulong channel, string prefix)
        {
            Token = token;
            Channel = channel;
            Prefix = prefix;
        }
    }
}
