using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TelegramBot {
    public static class Program {
        private const int NAME_MAX_LENGTH = 40;

        private static readonly TelegramBotClient Bot = new TelegramBotClient("API_KEY");

        public static void Main(string[] args) {
            try {
                var me = Bot.GetMeAsync().Result;
                Console.Title = me.Username;
                Bot.OnMessage += BotOnMessageReceived;
                Bot.StartReceiving(Array.Empty<UpdateType>());
                Console.WriteLine($"Start listening for @{me.Username}");
                Console.ReadLine();
                Bot.StopReceiving();
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadLine();
            }            
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs) {
            var message = messageEventArgs.Message;
            if (message == null) return;

            if (message.Type == MessageType.ChatMembersAdded) {
                foreach (Telegram.Bot.Types.User user in message.NewChatMembers) {
                    if (user != null) {
                        String name = "";
                        String separator = "";

                        if (!String.IsNullOrEmpty(user.FirstName)) {
                            name += user.FirstName;
                            separator = " ";
                        }
                        if (!String.IsNullOrEmpty(user.LastName)) {
                            name += separator + user.LastName;
                        }

                        Console.WriteLine($"New User: {name} Id:{user.Id} Language: {user.LanguageCode} Username: {user.Username}");

                        if (user.IsBot) {
                            Console.WriteLine($"SPAM Bot: {name} Id:{user.Id}");
                            await BlockSpamAsync(message.Chat.Id, user.Id, message.MessageId);
                        } else {
                            if (name.Length > NAME_MAX_LENGTH) {
                                Console.WriteLine($"SPAM User: {name} Id:{user.Id}");
                                await BlockSpamAsync(message.Chat.Id, user.Id, message.MessageId);
                            }
                        }
                    }
                }
            } else if (message.Type == MessageType.Text) {
                Console.WriteLine($"Chat: {message.Text}");
            }            
        }

        private static async System.Threading.Tasks.Task BlockSpamAsync(long chatId, int userId, int messageId) {
            await Bot.KickChatMemberAsync(chatId, userId, DateTime.Now.AddYears(2));
            await Bot.DeleteMessageAsync(chatId, messageId);
        }
    }
}
