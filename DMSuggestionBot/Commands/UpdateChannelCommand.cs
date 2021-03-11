using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DMSuggestionBot.Commands
{
    public class UpdateChannelCommand : BaseCommandModule
    {
        private readonly DiscordBot _bot;
        public UpdateChannelCommand(DiscordBot bot)
            => _bot = bot;

        [Command("channel")]
        [Description("Sets the channel to receive suggestions in.")]
        [Aliases("chan")]
        [RequireUserPermissions(Permissions.ManageChannels)]
        public async Task UpdateChannelCommandAsync(CommandContext ctx, 
            [Description("The new channel to send suggestions to")]
            DiscordChannel channel)
        {
            if(!channel.PermissionsFor(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id)).HasPermission(Permissions.SendMessages))
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.DarkRed)
                    .WithDescription("Can not set suggestions in that channel, we don't have permissions to send messages there!"));
                return;
            }

            await _bot.ReRegisterChannel(channel);

            await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.DarkGreen)
                .WithDescription("Channel updated successfully"));
        }
    }
}
