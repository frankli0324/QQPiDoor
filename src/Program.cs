using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace qdcontroller {

    class Program {

        public static void LogToConsole ( // logs are colorful
            string message,
            ConsoleColor textColor
        ) {
            Console.ForegroundColor = textColor;
            Console.WriteLine (message);
            Console.ResetColor ();
        }

        public static string OnTextMessage (string identifier, string message) {
            Program.LogToConsole (
                $"[{System.DateTime.Now.ToLocalTime().ToString("R")}]",
                ConsoleColor.DarkGreen
            );
            Program.LogToConsole (
                $"received request from {identifier}",
                ConsoleColor.Cyan
            );
            if (message.StartsWith ("register ")) {
                if (!MacChecker.Register (identifier, message.Substring (9))) {
                    return "mac地址的格式好像不太对emmmmmmm";
                }
                LogToConsole (
                    $"{identifier} registered",
                    ConsoleColor.DarkMagenta
                );
                return "[应该是]注册成功[了吧]";
            }
            if (!MacChecker.Registered (identifier)) {
                Program.LogToConsole ($"unregistered", ConsoleColor.Cyan);
                return
                @"要开门可是要注册的= =
注册格式:
register {设备的mac地址或你的设备名}(不带大括号)

*  多次注册会覆盖上次的请求
** 设备名可以允许一定的错误，但输入的设备名与真实设备名的<编辑距离>不能超过3
** 比如若你的设备名为qwert，那你可以输入qwe(2),qwasf(2),qwer(1),qe(3)
***。。。是不是设备名也没几个人会记。。。。。。。";
            }
            bool isOnline = false;
            using (var i = new HttpClient ()) {
                Console.WriteLine ("requesting");
                string authResponse = i.GetStringAsync (Config.global.AuthURL).Result;
                Console.WriteLine ("router replied");
                var onlineArr = JArray.Parse (authResponse);
                string userMac = MacChecker.MacUppercase (identifier);
                foreach (var item in onlineArr) {
                    if (userMac == item["mac"].ToString ()) {
                        isOnline = true;
                        LogToConsole ("MAC Hit!", ConsoleColor.Yellow);
                        break;
                    }
                }
            }
            if (!isOnline) {
                LogToConsole ("request blocked", ConsoleColor.DarkRed);
                return $"恶人!\n(你的mac地址:{MacChecker.MacUppercase(identifier)}";
            }
            var useless_variable = DoorKeeper.OpenDoor ();
            ///////////////////////////all clear///////////////////////////////
            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////
            return "走嘞! 您";
        }
        static void Main (string[] args) {
            Config.Load ();
            MacChecker.LoadRegisterList ();
            QQPart.Load ();
            TGPart.Load ();
            Console.WriteLine ("press enter to exit");

            Console.ReadLine ();
            MacChecker.GracefulExit ();
        }
    }

}