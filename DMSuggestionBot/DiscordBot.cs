using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DMSuggestionBot.Core;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Logging;

namespace DMSuggestionBot
{
    public class DiscordBot
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private DiscordChannel? SuggestionsChannel { get; set; }

        public DiscordBot(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            _client.MessageCreated += Client_MessageCreated;
            _client.Ready += Client_Ready;

            var cnext = await _client.UseCommandsNextAsync(new()
            {
                StringPrefixes = new string[] { Program.Config.Prefix },
                IgnoreExtraArguments = true,
                Services = _services
            });

            foreach(var c in cnext.Values)
            {
                c.RegisterCommands(Assembly.GetExecutingAssembly());

                c.CommandErrored += CNext_CommandErrored;
            }
        }

        public async Task StartAsync()
        {
            await _client.StartAsync();
        }

        public async Task ReRegisterChannel(DiscordChannel c)
        {
            SuggestionsChannel = c;
            Program.Config.Channel = c.Id;
            var json = JsonSerializer.Serialize(Program.Config, typeof(BotConfig));
            await File.WriteAllTextAsync(Path.Join("Config", "bot_config.json"), json);
        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            if (SuggestionsChannel is null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        SuggestionsChannel = await sender.GetChannelAsync(Program.Config.Channel);
                    }
                    catch
                    {
                        sender.Logger.LogInformation($"Failed to get suggestions channel for shard {sender.ShardId}");
                    }
                });
            }

            return Task.CompletedTask;
        }

        private Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if(e.Guild is null && SuggestionsChannel is not null)
            {
                _ = Task.Run(async () =>
                {
                    if (e.Author.IsBot || (e.Author.IsSystem ?? false)) return;

                    sender.Logger.LogDebug($"Sending suggestion from {e.Author.Username}");

                    await SuggestionsChannel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent(e.Message.Content)
                        .WithEmbed(new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.DarkBlue)
                            .WithDescription($"Suggestion received on: {DateTime.UtcNow:U}")));

                    await e.Channel.SendMessageAsync(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.DarkGreen)
                        .WithDescription("Suggestion sent!"));
                });
            }

            return Task.CompletedTask;
        }

        private Task CNext_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {

            _ = Task.Run(async () =>
            {
                sender.Client.Logger.LogError($"Command: {e.Command.Name} failed when executed by {e.Context.User.Username}");

                await e.Context.Channel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent("A command failed to execute: ")
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithColor(DiscordColor.DarkRed)
                        .WithDescription($"```{e.Exception.StackTrace}```")
                        .WithTitle(e.Exception.Message)));
            });

            return Task.CompletedTask;
        }
    }
}
