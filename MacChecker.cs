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
        static Dictionary<long, string> registerList = new Dictionary<long, string> ();

        public static void LoadRegisterList () {
            EnsureExist ();
            string regListArray =
                File.ReadAllText ("reglist.json");
            for (; regListArray.Length > 2;) {
                int semicolonPos = regListArray.IndexOf (';');

                var j = JToken
                    .Parse (regListArray.Substring (0, semicolonPos));
                Console.WriteLine ($"loaded:{j["qq"].ToObject<long> ()}:{j["mac"].ToString ()}");
                registerList[j["qq"].ToObject<long> ()] = j["mac"].ToObject<string> ();
                regListArray = regListArray.Substring (semicolonPos + 1);
            }
        }
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");

        public static bool Register (long qq_id, string mac_addr) {
            EnsureExist ();
            if (macReg.IsMatch (mac_addr) == false) return false;
            registerList[qq_id] = macReg.Match (mac_addr).Groups[0].Value;
            File.AppendAllText ("reglist.json", $"{{\"qq\":{qq_id},\"mac\":\"{registerList[qq_id]}\"}};");
            return true;
        }
        public static bool Registered (long qq_id) => registerList.ContainsKey (qq_id);
        public static string MacUppercase (long qq_id) => registerList[qq_id].ToUpper ();
        public static void GracefulExit () {
            File.Create ("reglist.json").Close ();
            foreach (var i in registerList)
                File.AppendAllText ("reglist.json", $"{{\"qq\":{i.Key},\"mac\":\"{i.Value}\"}};");

        }
    }
}