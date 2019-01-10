using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQResponses;
using cqhttp.Cyan.Instance;

namespace qdcontroller {
    public class QQPart {
        static CQApiClient CQCli = new CQHTTPClient (
            accessUrl: Config.global.ApiAddr,
            listen_port: Config.global.ListenPort,
            secret: Config.global.Secret
        );
        public static void Load () {
            CQCli.OnEventAsync += async (Api, e) => {
                if (!(e is PrivateMessageEvent)) return new EmptyResponse ();
                var sender = (e as PrivateMessageEvent).sender;
                var message = (e as PrivateMessageEvent).message;
                string text_message;
                if (message.data[0].type == "text")
                    text_message = message.data[0].data["text"];
                else text_message = "any_message";
                await CQCli.SendTextAsync (
                    MessageType.private_,
                    sender.user_id,
                    Program.OnTextMessage ($"QQ{sender.user_id}", text_message)
                );
                return new EmptyResponse ();
            };
        }
    }
}