using TelegramTools;

namespace RailwayBot
{
    internal class Program
    {
        static async Task Main()
        {
            var telegramManager = new TelegramManager();
            telegramManager.StartBot();
        }
    }
}