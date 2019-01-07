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
                Console.WriteLine (regListArray.Substring (0, semicolonPos));
                var j = JToken
                    .Parse (regListArray.Substring (0, semicolonPos))
                    .ToObject<KeyValuePair<long, string>> ();
                if (registerList.ContainsKey (j.Key) == false)
                    registerList.Add (j.Key, j.Value);
                else registerList[j.Key] = j.Value;
                regListArray = regListArray.Substring (semicolonPos + 1);
            }
        }
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");

        public static bool Register (long qq_id, string mac_addr) {
            EnsureExist ();
            if (macReg.IsMatch (mac_addr) == false) return false;
            if (registerList.ContainsKey (qq_id))
                registerList.Add (qq_id, mac_addr);
            else registerList[qq_id] = mac_addr;

            File.WriteAllText ("reglist.json", $"{{\"{qq_id}\":\"{mac_addr}\"}};");
            return true;
        }
        public static bool Registered (long qq_id) => registerList.ContainsKey (qq_id);
        public static string MacUppercase (long qq_id) => registerList[qq_id];
        public static void GracefulExit () {
            File.Create ("reglist.json").Close ();
            foreach (var i in registerList) 
                File.WriteAllText ("reglist.json", $"{{\"{i.Key}\":\"{i.Value}\"}};");
            
        }
    }
}