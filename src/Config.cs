using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace qdcontroller {
    [JsonObject]
    public class Config {
        [JsonProperty ("API_ADDR")] public string ApiAddr;
        [JsonProperty ("LISTEN_PORT")] public int ListenPort;
        [JsonProperty ("SECRET")] public string Secret;
        [JsonProperty ("AUTH_URL")] public string AuthURL;
        [JsonProperty ("TG_TOKEN")] public string TGToken;
        [JsonProperty ("PROXY_PORT")] public int ProxyPort;
        static string ConfigFilePath = "/home/pi/door.conf";
        public static Config global;
        public static void Load () {
            string conf = File.ReadAllText (ConfigFilePath);
            try {
                global = JToken.Parse (conf).ToObject<Config> ();
            } catch (System.Exception) {
                Program.LogToConsole ("create a configuration file", System.ConsoleColor.Red);
            }
            System.Console.WriteLine ("loaded" + global.ProxyPort);
        }
    }

}