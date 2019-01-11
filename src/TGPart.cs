using MihaZupan;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace qdcontroller {
    public class TGPart {
        static HttpToSocks5Proxy proxy =
            new HttpToSocks5Proxy ("127.0.0.1", Config.global.ProxyPort);
        static TelegramBotClient client;
        public static void Load () {
            client = new TelegramBotClient (Config.global.TGToken, proxy);
            client.OnMessage += async (sender, m) => {
                if (m.Message.Chat.Type != ChatType.Private) return;
                string text_message;
                if (m.Message.Type == MessageType.Text)
                    text_message = m.Message.Text;
                else text_message = "some_message";
                await client.SendTextMessageAsync (
                    m.Message.Chat.Id,
                    Program.OnTextMessage ("TG" + m.Message.From.Id, text_message),replyMarkup:new ReplyKeyboardMarkup(new KeyboardButton("开门!"))
                );
            };
            client.StartReceiving ();
        }
    }
}