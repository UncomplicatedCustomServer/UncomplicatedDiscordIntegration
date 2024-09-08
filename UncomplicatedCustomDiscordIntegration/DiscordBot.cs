using Exiled.API.Features;
using Newtonsoft.Json;
using SimpleDiscord;
using SimpleDiscord.Components;
using SimpleDiscord.Components.Builders;
using SimpleDiscord.Enums;
using SimpleDiscord.Events.Attributes;
using SimpleDiscord.Gateway;
using SimpleDiscord.Gateway.Events;
using SimpleDiscord.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedCustomDiscordIntegration.API.Features;

namespace UncomplicatedCustomDiscordIntegration
{
    internal class DiscordBot
    {
        private readonly Client client;

        internal readonly Dictionary<Enums.ChannelType, GuildTextChannel> channels = [];

        private readonly Dictionary<Enums.ChannelType, List<LogMessage>> logEntriesQueue = [];

        private readonly long Start;

        internal Guild Guild { get; private set; }

        public DiscordBot(string token, GatewayIntents intents = Gateway.defaultIntents)
        {
            client = new(new()
            {
                SaveGuildRegisteredCommands = false,
                SaveMessages = false,
                FetchThreadMembers = false,
                LoadMembers = true,
                LoadAppInfo = false,
                RegisterCommands = RegisterCommandType.CreateAndSkip
            });
            client.Logger.SubstituteLogHandler = (LogEntry log) =>
            {
                switch (log.Level)
                {
                    case LogLevel.Debug:
                        Exiled.API.Features.Log.Debug(log.Content);
                        break;
                    case LogLevel.Info:
                        Exiled.API.Features.Log.Info(log.Content);
                        break;
                    case LogLevel.Warn:
                        Exiled.API.Features.Log.Warn(log.Content);
                        break;
                    case LogLevel.Error:
                        Exiled.API.Features.Log.Error(log.Content);
                        break;
                }

                return log.Level is LogLevel.None;
            };
            client.EventHandler.RegisterEvents(this);
            client.LoginAsync(token, intents);
            Start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        [SocketEvent("READY")]
        public async void OnReady()
        {
            UpdateStatusActor();
            // Wait for a couple of seconds as we have to be uwu
            await Task.Delay(1500);

            Exiled.API.Features.Log.Warn(Guild.List.Count);
            foreach (Guild guild in Guild.List)
                Exiled.API.Features.Log.Warn($"{guild.Id} - {guild.Name}");

            if (Plugin.Instance.Config.Guild is 0)
            {
                Exiled.API.Features.Log.Error("Guild ID is not valid!");
                return;
            }

            Guild = Guild.GetGuild(Plugin.Instance.Config.Guild);

            if (Guild is null)
            {
                Exiled.API.Features.Log.Error($"Guild {Plugin.Instance.Config.Guild} not found!");
                return;
            }

            channels.Add(Enums.ChannelType.Command, Guild.GetChannel(Plugin.Instance.Config.Channels.Commands) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.Command, []);

            channels.Add(Enums.ChannelType.GameEvents, Guild.GetChannel(Plugin.Instance.Config.Channels.GameEvents) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.GameEvents, []);

            channels.Add(Enums.ChannelType.StaffCopy, Guild.GetChannel(Plugin.Instance.Config.Channels.StaffChannel) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.StaffCopy, []);

            channels.Add(Enums.ChannelType.Watchlist, Guild.GetChannel(Plugin.Instance.Config.Channels.Watchlist) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.Watchlist, []);

            channels.Add(Enums.ChannelType.Bans, Guild.GetChannel(Plugin.Instance.Config.Channels.Bans) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.Bans, []);

            channels.Add(Enums.ChannelType.Errors, Guild.GetChannel(Plugin.Instance.Config.Channels.Errors) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.Errors, []);

            channels.Add(Enums.ChannelType.Reports, Guild.GetChannel(Plugin.Instance.Config.Channels.Reports) as GuildTextChannel);
            logEntriesQueue.Add(Enums.ChannelType.Reports, []);

            await channels[Enums.ChannelType.GameEvents].SendMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle("Server successfully started!").SetColor("#63c930").SetDescription("")));
            client.RegisterCommand(ApplicationCommandBuilder.New("players", "Shows every connected player"));
        }

        [CommandHandler("players")]
        public static void OnPlayerCommand(Interaction interaction)
        {
            string players = $"**Online players [{Server.PlayerCount}/{Server.MaxPlayerCount}]:**\n";

            foreach (Player player in Player.List)
                players += $"\n`[{player.Id}]` **{player.Nickname}** `{player.UserId}` - {player.Role}";

            interaction.AcknowledgeWithMessage(MessageBuilder.New().SetContent(players));
        }

        public void AddMessageToQueue(LogMessage message)
        {
            List<LogMessage> instance = logEntriesQueue[message.ChannelType];

            string content = string.Join("\n", instance);

            if ($"{content}\n{message}".Length > 1995)
            {
                // We have to send messages, clear the queue and reset it
                channels[message.ChannelType].SendMessage(MessageBuilder.New().SetContent(content));
                logEntriesQueue[message.ChannelType].Clear();
                logEntriesQueue[message.ChannelType].Add(message);
                return;
            }

            if (instance.Count >= Plugin.Instance.Config.Bot.BucketSize)
            {
                channels[message.ChannelType].SendMessage(MessageBuilder.New().SetContent(content));
                logEntriesQueue[message.ChannelType].Clear();
                logEntriesQueue[message.ChannelType].Add(message);
                return;
            }

            // Add
            logEntriesQueue[message.ChannelType].Add(message);

            if (logEntriesQueue.Count >= Plugin.Instance.Config.Bot.BucketSize)
            {
                channels[message.ChannelType].SendMessage(MessageBuilder.New().SetContent($"{content}\n{message}"));
                logEntriesQueue[message.ChannelType].Clear();
                return;
            }
        }

        public async void UpdateStatusActor()
        {
            while (client.ConnectionStatus is ConnectionStatus.Connected or ConnectionStatus.Connecting)
            {
                client.Presence = new([
                    ActivityBuilder.New(Plugin.Instance.Config.Bot.Presence.Replace("%current%", Player.List.Count.ToString()).Replace("%total%", Server.MaxPlayerCount.ToString()))
                    .SetType(Plugin.Instance.Config.Bot.PresenceActivity)
                    .SetDetails("UWU v2v2")
                    .SetCreatedAt(Start)
                ], Player.List.Count switch
                {
                    0 => "idle",
                    _ => "online"
                });
                await Task.Delay(1500);
            }
        }

        public async Task SendMessage(GuildTextChannel channel, SocketSendMessage message) => await channel.SendMessage(message);

        internal void Close() => client.Disconnect();
    }
}
