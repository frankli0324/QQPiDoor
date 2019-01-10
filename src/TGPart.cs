using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace qdcontroller {
    public class TGPart {
        static TelegramBotClient client = new TelegramBotClient (Config.global.TGToken);
        public static void Load () {
            client.OnMessage += async (sender, m) => {
                if (m.Message.Chat.Type != ChatType.Private) return;
                string text_message;
                if (m.Message.Type == MessageType.Text)
                    text_message = m.Message.Text;
                else text_message = "some_message";
                await client.SendTextMessageAsync (
                    m.Message.Chat.Id,
                    Program.OnTextMessage ("TG" + m.Message.From.Id, text_message)
                );
            };
        }
    }
}