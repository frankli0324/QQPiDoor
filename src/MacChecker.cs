using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace qdcontroller {
    public class MacChecker {
        static long Min (long a, long b) => a < b?a : b;
        static void EnsureExist () {
            if (!File.Exists ("reglist.json")) {
                File.Create ("reglist.json").Close ();
            }
        }
        static long[][] dp;
        public static long GetStringEditDistance (string a, string b) {
            dp = new long[2][];
            dp[0] = new long[b.Length + 1];
            dp[1] = new long[b.Length + 1];

            for (int i = 0; i <= a.Length; i++) {
                for (int j = 0; j <= b.Length; j++) {
                    if (i == 0 && j == 0) dp[i & 1][j] = 0;
                    else if (i == 0) dp[0][j] = j;
                    else if (j == 0) dp[i & 1][0] = i;
                    else if (a[i - 1] == b[j - 1]) dp[i & 1][j] = Min (dp[(i - 1) & 1][j - 1], Min (dp[(i - 1) & 1][j], dp[i & 1][j - 1]) + 1);
                    else dp[i & 1][j] = Min (dp[(i - 1) & 1][j - 1], Min (dp[(i - 1) & 1][j], dp[i & 1][j - 1])) + 1;
                }
            }
            return dp[a.Length & 1][b.Length];
        }
        static Dictionary<string, string> registerList = new Dictionary<string, string> ();
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");
        public static bool Registered (string id) => registerList.ContainsKey (id);
        public static string MacUppercase (string id) => registerList[id];
        public static bool Register (string id, string mac_addr) {
            EnsureExist ();
            if (macReg.IsMatch (mac_addr) == false) {
                mac_addr = TryGetMacByName (mac_addr);
                if (mac_addr == "not_found") return false;
            }
            registerList[id] = macReg.Match (mac_addr).Groups[0].Value.ToUpper ();
            if (id.StartsWith ("QQ"))
                File.AppendAllText (
                    "reglist.json",
                    $"{{\"qq\":{id.Substring(2)},\"mac\":\"{registerList[id]}\"}};"
                );
            else //if(id.StartsWith("TG"))
                File.AppendAllText (
                    "reglist.json",
                    $"{{\"tg\":{id.Substring(2)},\"mac\":\"{registerList[id]}\"}};"
                );
            return true;
        }
        public static string TryGetMacByName (string name) {
            using (var i = new HttpClient ()) {
                Console.WriteLine ("requesting");
                string authResponse = i.GetStringAsync (Config.global.AuthURL).Result;
                Console.WriteLine ("router replied");
                var onlineArr = JArray.Parse (authResponse);
                foreach (var item in onlineArr) {
                    if (GetStringEditDistance (name, item["devicename"].ToString ()) < 3) {
                        return item["mac"].ToObject<string> ();
                    }
                }
            }
            return "not_found";
        }
        public static void LoadRegisterList () {
            EnsureExist ();
            string regListArray =
                File.ReadAllText ("reglist.json");
            for (; regListArray.Length > 2;) {
                int semicolonPos = regListArray.IndexOf (';');

                var j = JToken
                    .Parse (regListArray.Substring (0, semicolonPos));
                if (j["tg"] != null)
                    registerList["TG" + j["tg"].ToObject<string> ()] = j["mac"].ToObject<string> ();
                else if (j["qq"] != null)
                    registerList["QQ" + j["qq"].ToObject<string> ()] = j["mac"].ToObject<string> ();
                else throw new Exception ();
                Console.WriteLine ($"loaded: \"{j["mac"].ToString ()}\"");
                regListArray = regListArray.Substring (semicolonPos + 1);
            }
        }
        public static void GracefulExit () {
            File.Create ("reglist.json").Close ();
            foreach (var i in registerList)
                if (i.Key.StartsWith ("QQ"))
                    File.AppendAllText (
                        "reglist.json",
                        $"{{\"qq\":{i.Key.Substring(2)},\"mac\":\"{i.Value}\"}};"
                    );
                else //if(id.StartsWith("TG"))
                    File.AppendAllText (
                        "reglist.json",
                        $"{{\"tg\":{i.Key.Substring(2)},\"mac\":\"{i.Value}\"}};"
                    );

        }
    }
}