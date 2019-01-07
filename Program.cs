using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQResponses;
using cqhttp.Cyan.Instance;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace qdcontroller {

    class Program {
        static string help = "you forgot to set the env variable";
        static Dictionary<long, string> registerList = new Dictionary<long, string> ();
        static void LogToConsole ( // logs are colorful
            string message,
            ConsoleColor textColor
        ) {
            Console.ForegroundColor = textColor;
            Console.WriteLine (message);
            Console.ResetColor ();
        }
        static void EnsureExist () {
            if (!File.Exists ("reglist.json")) {
                File.Create ("reglist.json").Close ();
            }
        }
        static void LoadRegisterList () {
            string regListArray =
                File.ReadAllText ("reglist.json");
            for (int i = 0; i < regListArray.Length;) {
                int semicolonPos = regListArray.IndexOf (';');
                var j = JToken
                    .Parse (regListArray.Substring (0, semicolonPos))
                    .ToObject < (long, string) > ();
                registerList.Add (j.Item1, j.Item2);
                i = semicolonPos + 1;
            }
        }
        static Regex macReg = new Regex ("([0-9A-f]{2}:){5}[0-9A-f]{2}");
        static bool Register (long qq_id, string mac_addr) {
            if (macReg.IsMatch (mac_addr) == false) return false;
            if (registerList.ContainsKey (qq_id))
                registerList.Add (qq_id, mac_addr);
            else registerList[qq_id] = mac_addr;

            File.WriteAllText ("reglist.json", $"{{\"{qq_id}\",\"{mac_addr}\"}};");
            return true;
        }

        static string ApiAddr, ListenPort, LogDebug, AuthURL;
        static void Main (string[] args) {
            EnsureExist ();
            ApiAddr = args[0].Length != 0 ?
                args[0] : System.Environment.GetEnvironmentVariable ("API_ADDR");
            ListenPort = args[1].Length != 0 ?
                args[1] : System.Environment.GetEnvironmentVariable ("LISTEN_PORT");
            AuthURL = args[2].Length != 0 ?
                args[2] : System.Environment.GetEnvironmentVariable ("AUTH_URL");
            LogDebug = System.Environment.GetEnvironmentVariable ("IS_DEBUG");

            LoadRegisterList ();
            if (!(LogDebug is null))
                Logger.verbosity_level = Verbosity.DEBUG;
            if (false && (ApiAddr is null || ListenPort is null || AuthURL is null)) {
                Console.WriteLine (help);
                return;
            }
            //////////////////////////env check finished///////////////////////////////////
            var CQCli = new CQHTTPClient (
                accessUrl: ApiAddr,
                listen_port: int.Parse (ListenPort),
                secret: "bighacker"
            );
            CQCli.OnEventAsync += async (Api, e) => {
                if (!(e is PrivateMessageEvent)) return new EmptyResponse ();
                var sender = (e as PrivateMessageEvent).sender;
                var message = (e as PrivateMessageEvent).message;
                if (message.data[0].type == "text" &&
                    message.data[0].data["text"].StartsWith ("register")
                ) {
                    if (!Register (sender.user_id, message.data[0].data["text"].Substring (9))) {
                        await CQCli.SendTextAsync (
                            MessageType.private_,
                            sender.user_id, "mac地址的格式好像不太对emmmmmmm"
                        );
                        return new EmptyResponse ();
                    }

                    await CQCli.SendTextAsync (
                        MessageType.private_,
                        sender.user_id, "[应该是]注册成功[了吧]"
                    );
                    LogToConsole (
                        $"{sender.user_id} registered",
                        ConsoleColor.DarkMagenta
                    );
                    return new EmptyResponse ();
                }
                LogToConsole (
                    $"[{System.DateTime.Now.ToLocalTime().ToString("R")}]",
                    ConsoleColor.DarkGreen
                );
                LogToConsole (
                    $"received request from {sender.user_id}",
                    ConsoleColor.Cyan
                );

                ///////////////////////////begin online check///////////////////////////////
                bool isOnline = false;
                if (!registerList.ContainsKey (sender.user_id)) {
                    LogToConsole ($"unregistered", ConsoleColor.Cyan);

                    await CQCli.SendTextAsync (
                        MessageType.private_,
                        sender.user_id,
                        @"要开门可是要注册的= =
注册格式:
register {设备的mac地址}
多次注册会覆盖上次的请求"
                    );
                    return new EmptyResponse ();
                }
                using (var i = new HttpClient ()) {
                    var onlineArr = JArray.Parse (await i.GetStringAsync (AuthURL));
                    //Console.WriteLine(onlineArr["mac"]);
                    foreach (var item in onlineArr) {
                        if (registerList[sender.user_id].ToLower () == item["mac"].ToString ().ToLower ()) {
                            isOnline = true;
                            break;
                        }
                    }
                }

                if (!isOnline) {
                    await CQCli.SendTextAsync (
                        MessageType.private_,
                        sender.user_id,
                        $"恶人!\n(你的mac地址:{registerList[sender.user_id]}"
                    );
                    LogToConsole ("request blocked", ConsoleColor.DarkRed);
                    return new EmptyResponse ();
                }
                ///////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////
                var useless_variable = OpenDoor ();
                ///////////////////////////all clear///////////////////////////////
                ///////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////
                await Api.SendTextAsync (
                    MessageType.private_,
                    (e as PrivateMessageEvent).sender.user_id,
                    "走勒 您"
                );
                return new EmptyResponse ();
            };
            Console.WriteLine ("press enter to exit");
            Console.ReadLine ();
        }
        static object door_lock = new object ();
        static Random random = new Random (9123);
        static void OperateGPIO (string dest, string value) {
            File.WriteAllText ("/sys/class/gpio/" + dest, value);
        }
        static async Task OpenDoor () {
            await Task.Run (() => {
                lock (door_lock) {
                    if (!Directory.Exists ("/sys/class/gpio/gpio26")) {
                        Logger.Log (Verbosity.DEBUG, "export gpio 26");
                        OperateGPIO ("export", "26");
                    }
                    Console.WriteLine (random.Next (2) == 0 ? "Hacker time!QwQ" : "Open, Sesame!");

                    OperateGPIO ("gpio26/direction", "out");

                    OperateGPIO ("gpio26/value", "0");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "1");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "0");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "1");

                    if (Directory.Exists ("/sys/class/gpio/gpio26")) {
                        Logger.Log (Verbosity.DEBUG, "unexport gpio 26");
                        OperateGPIO ("unexport", "26");
                    }
                }
            });
        }
    }

}