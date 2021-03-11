using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using DMSuggestionBot.Core;

using DSharpPlus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DMSuggestionBot
{
    public class Program
    {
        public static BotConfig Config { get; private set; }

        public static void Main(string[] args)
        {
            Start(args).GetAwaiter().GetResult();
        }

        public static async Task Start(string[] args)
        {
            using (FileStream fs = new(Path.Join("Config", "bot_config.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var c = await JsonSerializer.DeserializeAsync<BotConfig>(fs);
                if (c is null)
                    throw new Exception("Config file cannot be null");

                Config = c;
            }

            ServiceCollection collection = new ServiceCollection();
            collection.AddSingleton<DiscordBot>()
                .AddSingleton<DiscordShardedClient>(x =>
                {
                    return new(new()
                    {
                        Token = Config.Token,
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.DirectMessages
                            | DiscordIntents.Guilds
                            | DiscordIntents.GuildMessages,
#if DEBUG
                        MinimumLogLevel = LogLevel.Debug,
#else
                        MinimumLogLevel = LogLevel.Information,
#endif
                    });
                });

            IServiceProvider provider = collection.BuildServiceProvider();
            var bot = provider.GetRequiredService<DiscordBot>();

            await bot.InitializeAsync();
            await bot.StartAsync();

            await Task.Delay(-1);
        }
    }
}
