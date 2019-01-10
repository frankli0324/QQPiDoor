using Newtonsoft.Json;

namespace qdcontroller {
    [JsonObject]
    public class Config {
        [JsonProperty ("API_ADDR")] public string ApiAddr;
        [JsonProperty ("LISTEN_PORT")] public int ListenPort;
        [JsonProperty ("SECRET")] public string Secret;
        [JsonProperty ("AUTH_URL")] public string AuthURL;
        [JsonProperty ("TG_TOKEN")] public string TGToken;
        public static Config global;
    }

}