using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace qdcontroller {
    public class MacChecker {
        static void EnsureExist () {
            if (!File.Exists ("reglist.json")) {
                File.Create ("reglist.json").Close ();
            }
        }
        static Dictionary<string, string> registerList = new Dictionary<string, string> ();

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
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");

        public static bool Register (string id, string mac_addr) {
            EnsureExist ();
            if (macReg.IsMatch (mac_addr) == false) return false;
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
        public static bool Registered (string id) => registerList.ContainsKey (id);
        public static string MacUppercase (string id) => registerList[id];
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