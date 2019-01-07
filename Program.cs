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
        static void LogToConsole ( // logs are colorful
            string message,
            ConsoleColor textColor
        ) {
            Console.ForegroundColor = textColor;
            Console.WriteLine (message);
            Console.ResetColor ();
        }

        static string ApiAddr, ListenPort, AuthURL;
        static void Main (string[] args) {
            ApiAddr = args[0].Length != 0 ?
                args[0] : System.Environment.GetEnvironmentVariable ("API_ADDR");
            ListenPort = args[1].Length != 0 ?
                args[1] : System.Environment.GetEnvironmentVariable ("LISTEN_PORT");
            AuthURL = args[2].Length != 0 ?
                args[2] : System.Environment.GetEnvironmentVariable ("AUTH_URL");
            MacChecker.LoadRegisterList ();

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
                    if (!MacChecker.Register (sender.user_id, message.data[0].data["text"].Substring (9))) {
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
                if (!MacChecker.Registered (sender.user_id)) {
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
                bool isOnline = false;
                using (var i = new HttpClient ()) {
                    var onlineArr = JArray.Parse (await i.GetStringAsync (AuthURL));
                    //Console.WriteLine(onlineArr["mac"]);
                    foreach (var item in onlineArr) {
                        if (MacChecker.MacUppercase (sender.user_id) == item["mac"].ToString ()) {
                            isOnline = true;
                            break;
                        }
                    }
                }

                if (!isOnline) {
                    await CQCli.SendTextAsync (
                        MessageType.private_,
                        sender.user_id,
                        $"恶人!\n(你的mac地址:{MacChecker.MacUppercase(sender.user_id)}"
                    );
                    LogToConsole ("request blocked", ConsoleColor.DarkRed);
                    return new EmptyResponse ();
                }
                ///////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////
                var useless_variable = DoorKeeper.OpenDoor ();
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
            MacChecker.GracefulExit ();
            Console.ReadLine ();
        }
    }

}