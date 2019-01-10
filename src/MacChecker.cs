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
                Console.WriteLine ($"loaded:{j["qq"].ToObject<string> ()}:{j["mac"].ToString ()}");
                registerList[j["qq"].ToObject<string> ()] = j["mac"].ToObject<string> ();
                regListArray = regListArray.Substring (semicolonPos + 1);
            }
        }
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");

        public static bool Register (string id, string mac_addr) {
            EnsureExist ();
            if (macReg.IsMatch (mac_addr) == false) return false;
            registerList[id] = macReg.Match (mac_addr).Groups[0].Value;
            File.AppendAllText ("reglist.json", $"{{\"qq\":{id},\"mac\":\"{registerList[id]}\"}};");
            return true;
        }
        public static bool Registered (string id) => registerList.ContainsKey (id);
        public static string MacUppercase (string id) => registerList[id].ToUpper ();
        public static void GracefulExit () {
            File.Create ("reglist.json").Close ();
            foreach (var i in registerList)
                File.AppendAllText ("reglist.json", $"{{\"qq\":{i.Key},\"mac\":\"{i.Value}\"}};");

        }
    }
}