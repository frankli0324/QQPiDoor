using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQEvents.CQResponses;
using cqhttp.Cyan.Instance;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace qdcontroller {

    class Program {
        static string AuthURL = "";
        static void LogToConsole (
            string message,
            ConsoleColor textColor
        ) {
            Console.ForegroundColor = textColor;
            Console.WriteLine (message);
            Console.ResetColor ();
        }
        static string help = "\nsomething's wrong with the env.\nplease exec\n\"export API_ADDR={here goes coolq api addr}\" and \n\"export LISTEN_PORT={port to listen on}\"before executing this hacker thing.";
        static string ApiAddr, ListenPort, LogDebug;
        static void Main () {
            ApiAddr = System.Environment.GetEnvironmentVariable ("API_ADDR");
            ListenPort = System.Environment.GetEnvironmentVariable ("LISTEN_PORT");
            LogDebug = System.Environment.GetEnvironmentVariable ("IS_DEBUG");
            if (!(LogDebug is null))
                Logger.verbosity_level = cqhttp.Cyan.Enums.Verbosity.DEBUG;
            if (ApiAddr is null || ListenPort is null) {
                Console.WriteLine (help);
                return;
            }
            var CQCli = new CQHTTPClient (
                accessUrl: ApiAddr,
                //accessToken: "qwertisverysao",
                listen_port : int.Parse (ListenPort),
                secret: "bighacker"
            );
            Task.Run (async () => {
                JArray authList;
                while (true) {
                    Thread.Sleep (2000);
                    using (var i = new HttpClient ()) {
                        string authListStr = await i.GetStringAsync (AuthURL);
                        authList = JArray.Parse (authListStr);
                    }
                    
                }
            });
            CQCli.OnEventDelegate += (Api, e) => {
                if (!(e is PrivateMessageEvent)) return new EmptyResponse ();
                var sender = (e as PrivateMessageEvent).sender;
                LogToConsole (
                    $"[{System.DateTime.Now.ToLocalTime().ToString("R")}]",
                    ConsoleColor.DarkGreen
                );
                LogToConsole (
                    $"received request from {sender.user_id}",
                    ConsoleColor.Cyan
                );
                var a_variable_which_is_not_important_at_all = OpenDoor ();
                Api.SendTextAsync (
                    cqhttp.Cyan.Enums.MessageType.private_,
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
                        Logger.Log (cqhttp.Cyan.Enums.Verbosity.DEBUG, "export gpio 26");
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
                        Logger.Log (cqhttp.Cyan.Enums.Verbosity.DEBUG, "unexport gpio 26");
                        OperateGPIO ("unexport", "26");
                    }
                }
            });
        }
    }

}