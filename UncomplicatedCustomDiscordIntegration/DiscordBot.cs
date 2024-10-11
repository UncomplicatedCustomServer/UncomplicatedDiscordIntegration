using Exiled.API.Features;
using SimpleDiscord;
using SimpleDiscord.Components;
using SimpleDiscord.Components.Builders;
using SimpleDiscord.Components.DiscordComponents;
using SimpleDiscord.Enums;
using SimpleDiscord.Events.Attributes;
using SimpleDiscord.Gateway;
using SimpleDiscord.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UncomplicatedDiscordIntegration.API.Features;
using UncomplicatedDiscordIntegration.Extensions;
using UncomplicatedDiscordIntegration.Manager.NET;

namespace UncomplicatedDiscordIntegration
{
    internal class DiscordBot
    {
        internal readonly Client client;

        internal readonly Dictionary<Enums.ChannelType, GuildTextChannel> channels = [];

        private readonly Dictionary<long, List<LogMessage>> logEntriesQueue = [];

        private readonly long Start;

        internal Guild Guild { get; private set; }

        public DiscordBot(string token, GatewayIntents intents = Gateway.defaultIntents)
        {
            try
            {
                client = new(new()
                {
                    SaveGuildRegisteredCommands = false,
                    SaveMessages = false,
                    FetchThreadMembers = false,
                    LoadMembers = true,
                    LoadAppInfo = false,
                    RegisterCommands = RegisterCommandType.CreateAndEdit
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
            catch (Exception e)
            {
                client.Logger.Error(e.ToString());
            }
        }

        [SocketEvent("READY")]
        public async void OnReady()
        {
            UpdateStatusActor();
            // Wait for a couple of seconds as we have to be uwu
            await Task.Delay(3500);

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
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.Command].Id, []);

            channels.Add(Enums.ChannelType.GameEvents, Guild.GetChannel(Plugin.Instance.Config.Channels.GameEvents) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.GameEvents].Id, []);

            channels.Add(Enums.ChannelType.StaffCopy, Guild.GetChannel(Plugin.Instance.Config.Channels.StaffChannel) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.StaffCopy].Id, []);

            channels.Add(Enums.ChannelType.Watchlist, Guild.GetChannel(Plugin.Instance.Config.Channels.Watchlist) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.Watchlist].Id, []);

            channels.Add(Enums.ChannelType.Bans, Guild.GetChannel(Plugin.Instance.Config.Channels.Bans) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.Bans].Id, []);

            channels.Add(Enums.ChannelType.Errors, Guild.GetChannel(Plugin.Instance.Config.Channels.Errors) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.Errors].Id, []);

            channels.Add(Enums.ChannelType.Reports, Guild.GetChannel(Plugin.Instance.Config.Channels.Reports) as GuildTextChannel);
            logEntriesQueue.TryAdd(channels[Enums.ChannelType.Reports].Id, []);

            await channels[Enums.ChannelType.GameEvents].SendMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle("Server successfully started!").SetColor("#63c930").SetDescription("")));
            client.RegisterCommand(ApplicationCommandBuilder.New("players", "Shows every connected player"));
            client.RegisterCommand(ApplicationCommandBuilder.New("watchlist", "Manage the watchlist")
                .AddOption(CommandOptionBuilder.New("list", CommandOptionType.SUB_COMMAND, "See the watchlist")
                    .AddOption(CommandOptionBuilder.New("page", CommandOptionType.INTEGER, "The page of the list").SetRequired(false))
                )
                .AddOption(CommandOptionBuilder.New("add", CommandOptionType.SUB_COMMAND, "Add a user to the watchlist")
                    .AddOption(CommandOptionBuilder.New("steamid", CommandOptionType.STRING, "The SteamID (x64) of the user that needs to be watchlisted").SetRequired().MinLength(17).MaxLength(17))
                    .AddOption(CommandOptionBuilder.New("reason", CommandOptionType.STRING, "The reason for the insertion inside the watchlist").SetRequired())
                )
                .AddOption(CommandOptionBuilder.New("remove", CommandOptionType.SUB_COMMAND, "Remove a user from the watchlist")
                    .AddOption(CommandOptionBuilder.New("steamid", CommandOptionType.STRING, "The SteamID (x64) of the user that has to be removed.").SetRequired().MinLength(17).MaxLength(17))
                )
                .AddOption(CommandOptionBuilder.New("view", CommandOptionType.SUB_COMMAND, "View the details of a specific watchlist entry.")
                    .AddOption(CommandOptionBuilder.New("steamid", CommandOptionType.STRING, "The SteamID (x64) of the user.").SetRequired().MinLength(17).MaxLength(17))
                )
                .AddOption(CommandOptionBuilder.New("clear", CommandOptionType.SUB_COMMAND, "Clear every user from the watchlist."))
            );
        }

        [CommandHandler("players")]
        public static void OnPlayerCommand(Interaction interaction)
        {
            string players = $"**Online players [{Server.PlayerCount}/{Server.MaxPlayerCount}]:**\n";

            foreach (Player player in Player.List)
                players += $"\n`[{player.Id}]` **{player.Nickname}** `{player.UserId}` - {player.Role}";

            interaction.AcknowledgeWithMessage(MessageBuilder.New().SetContent(players));
        }

#nullable enable
        [CommandHandler("watchlist", "list")]
        public async void OnWatchlistListCommand(Interaction interaction)
        {
            if (interaction.Data is not ApplicationCommandInteractionData data)
                return;

            await interaction.AcknowledgeWithLoading(false);
            try
            {
                int page = (int)(data.GetOption("page")?.GetValue() ?? 1);

                if (page > 0)
                    page--;

                if (page < 0)
                    await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("Page can't be negative!"));
                else
                    HandleList(interaction, page);
            } 
            catch(Exception e)
            {
                client.Logger.Error(e.ToString());
            }
        }

        [CommandHandler("watchlist", "remove")]
        public static async void OnWatchlistRemoveCommand(Interaction interaction)
        {
            if (interaction.Data is not ApplicationCommandInteractionData data)
                return;

            await interaction.AcknowledgeWithLoading();

            if (data.TryGetOption("steamid", out ReplyCommandOption? option) && option is not null && option.GetValue() is string steamid)
            {
                await WatchlistManager.Remove(steamid);
                await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("User successfully removed from the watchlist"));
            }
        }

        [CommandHandler("watchlist", "view")]
        public void OnWatchlistViewCommand(Interaction interaction)
        {
            if (interaction.Data is not ApplicationCommandInteractionData data)
                return;

            if (data.TryGetOption("steamid", out ReplyCommandOption? option) && option is not null && option.GetValue() is string steamid)
                HandleUserInfo(interaction, steamid);
        }
#nullable disable

        [CommandHandler("watchlist", "add")]
        public async void OnWatchlistAddCommand(Interaction interaction)
        {
            await interaction.AcknowledgeWithLoading(false);

            if (interaction.Data is not ApplicationCommandInteractionData data)
                return;

            string steamid = (string)data.GetOption("steamid")?.GetValue();

            if (steamid is null)
                return;

            if (WatchlistEntry.TryCreateFromDiscord(interaction.Member.User, steamid, (string)data.GetOption("reason")?.GetValue(), out WatchlistEntry entry))
            {
                if (await WatchlistManager.Add(entry))
                    await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle($"{entry.Target} added to the watchlist!").SetDescription("").SetColor("#a13525").SetAuthor(string.Empty, string.Empty, "https://cdn.discordapp.com/emojis/835097985397817355.png?size=1024").SetThumbnail(entry.AvatarUrl).AddField("Date", $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:F>").AddField("User", $"`{entry.Target}` - `{entry.TargetId}`").AddField("Author", $"{interaction.Member} `{interaction.Member.User?.Username}` - `{entry.AuthorId}`").AddField("Reason", entry.Reason)));
                else
                    await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle($"Failed to update the watchlist!").SetDescription("Failed to add the user to the watchlist!\n\nCommon errors are:\n- the user is already in the watchlist\n- server can't reach the remote database").SetColor("#a13525").SetThumbnail("https://cdn.discordapp.com/emojis/835098401078116362.png?size=1024")));
            }
        }

        [CommandHandler("watchlist", "clear")]
        public async void OnWatchlistClearCommand(Interaction interaction)
        {
            await interaction.AcknowledgeWithLoading();

            await WatchlistManager.Clear();

            await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle("Watchlist cleared!").SetDescription("").SetThumbnail("https://cdn.discordapp.com/emojis/835097985397817355.png?size=1024").SetColor("#518a0f")));
        }

        [ComponentHandler("destroy_message")]
        public async void OnDeleteButton(Interaction interaction)
        {
            if (interaction.Message is not null)
                try
                {
                    await interaction.Message.Delete();
                } 
                catch (Exception e)
                {
                    client.Logger.Error(e.ToString());
                }
        }

        [ComponentHandler("userselect_seemorepls")]
        public void OnMoreInfoPage(Interaction interaction)
        {
            if (interaction.Data is not MessageComponentInteractionData data)
                return;

            if (data.Values[0] is null)
                return;

            HandleUserInfo(interaction, data.Values[0]);
        }

        public void AddMessageToQueue(LogMessage message)
        {
            if (message is null)
                return;

            try
            {
                if (channels.TryGetValue(message.ChannelType, out GuildTextChannel channel) && logEntriesQueue.TryGetValue(channel.Id, out List<LogMessage> instance))
                {
                    string content = string.Join("\n", instance);

                    if ($"{content}\n{message}".Length > 1995)
                    {
                        // We have to send messages, clear the queue and reset it
                        channel.SendMessage(MessageBuilder.New().SetContent(content));
                        logEntriesQueue[channel.Id].Clear();
                        logEntriesQueue[channel.Id].Add(message);
                        return;
                    }

                    if (instance.Count >= Plugin.Instance.Config.Bot.BucketSize)
                    {
                        channel.SendMessage(MessageBuilder.New().SetContent(content));
                        logEntriesQueue[channel.Id].Clear();
                        logEntriesQueue[channel.Id].Add(message);
                        return;
                    }

                    // Add
                    logEntriesQueue[channel.Id].Add(message);

                    if (logEntriesQueue.Count >= Plugin.Instance.Config.Bot.BucketSize)
                    {
                        channel.SendMessage(MessageBuilder.New().SetContent($"{content}\n{message}"));
                        logEntriesQueue[channel.Id].Clear();
                        return;
                    }
                }
                else
                    Exiled.API.Features.Log.Warn($"Channel for {message.ChannelType} not found!");
            }
            catch (Exception e)
            {
                Exiled.API.Features.Log.Error(e.ToString());
            }
        }

        internal void WatchlistUserJoined(WatchlistEntry entry)
        {
            channels[Enums.ChannelType.Watchlist].SendMessage(MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New()
                .SetTitle($"Watchlisted user {entry.Target} joined the server!")
                .SetDescription("")
                .AddField("User", $"`{entry.Target}` `{entry.TargetId}`")
                .AddField("Author", $"`{entry.Author}` `{entry.AuthorId}`")
                .AddField("Time", $"<t:{entry.Time}:F>")
                .AddField("Reason", entry.Reason)
                .SetThumbnail(entry?.AvatarUrl ?? "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/1024px-Steam_icon_logo.svg.png")
            ));
        }

        private static EmbedBuilder GenerateList(WatchlistEntry[] entries, EmbedBuilder embed, out List<SelectOption> options, int page = 0)
        {
            int limit = 20;
            options = [];
            for (int a = page * limit; a < entries.Length; a++)
            {
                WatchlistEntry entry = entries[a];
                options.Add(GenerateSelectMenu(entry));
                embed.AddField($"{entry.Target} `{entry.TargetId}`", entry.Reason);
            }
            embed.SetFooter($"Powered by UCS Collective • Page {page + 1}/{Math.Ceiling((decimal)entries.Length / limit)} • Showing 20/{entries.Length} entries", "https://ucs.fcosma.it/assets/forum/img/logo.png");
            return embed;
        }

        private static SelectOption GenerateSelectMenu(WatchlistEntry entry) => new(entry.Target, entry.TargetId, $"See the entry created by {entry.Author}", new(null, "ℹ"), false);

#nullable enable
        private async static void HandleList(Interaction interaction, int page = 0)
        {
            WatchlistEntry[]? entries = await WatchlistManager.Get();
            
            if (page*20 > entries?.Length)
            {
                await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent($"Page not found!"));
                return;
            }

            List<SelectOption> options = [];
            EmbedBuilder embed = entries?.Length < 1 ? EmbedBuilder.New().SetTitle("Watchlist Entries").SetDescription("No entry found!").SetColor("#538f2e") : EmbedBuilder.New().SetTitle("Watchlist Entries").SetDescription("").SetColor("#8f1f1f");
            if (entries is not null && entries?.Length > 0)
                embed = GenerateList(entries, embed, out options, page);

            MessageBuilder builder = MessageBuilder.New()
                .SetContent("")
                .AddEmbed(embed);

            if (entries?.Length > 0)
                builder.AddComponent(ActionRowBuilder.New()
                    .AddComponent(ButtonBuilder.New()
                        .SetLabel("")
                        .SetEmoji(new(null, "◀"))
                        .SetDisabled(page < 1)
                        .SetStyle(ButtonStyle.Primary)
                        .SetData(page--)
                        .SetCallback((Interaction interaction, object data) =>
                        {
                            if (data is not int newPage)
                                return;

                            interaction.AcknowledgeWithLoading();
                            HandleList(interaction, newPage);
                        })
                    )
                    .AddComponent(ButtonBuilder.New()
                        .SetLabel("Delete")
                        .SetStyle(ButtonStyle.Danger)
                        .SetCustomId("destroy_message")
                    )
                    .AddComponent(ButtonBuilder.New()
                        .SetLabel("").SetEmoji(new(null, "▶"))
                        .SetDisabled(entries?.Length <= 20)
                        .SetData(page++)
                        .SetCallback((Interaction interaction, object data) =>
                        {
                            if (data is not int newPage)
                                return;

                            interaction.AcknowledgeWithLoading();
                            HandleList(interaction, newPage);
                        })
                    )
                ).AddComponent(ActionRowBuilder.New().AddComponent(new TextSelectMenu([.. options], "userselect_seemorepls", "Select an user to see more about his entry", 1, 1, false)));

            await interaction.UpdateOriginalMessage(builder);
        }

        public async void HandleUserInfo(Interaction interaction, string steamid)
        {
            steamid = steamid.Contains("@steam") ? steamid : $"{steamid}@steam";

            await interaction.AcknowledgeWithLoading(false);
            
            try
            {
                WatchlistEntry? entry = await WatchlistManager.Get(steamid);

                if (entry is null)
                {
                    await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("")
                        .AddEmbed(EmbedBuilder.New().SetTitle("User not found!").SetColor("#a63a1f").SetDescription("").SetThumbnail("https://cdn.discordapp.com/emojis/835098401078116362.png?size=1024"))
                    );
                    return;
                }

                await interaction.UpdateOriginalMessage(MessageBuilder.New().SetContent("")
                    .AddEmbed(EmbedBuilder.New()
                        .SetTitle($"User {entry.Target}")
                        .AddField("User", $"`{entry.Target}` `{entry.TargetId}`", true)
                        .AddField("Author", $"`{entry.Author}` `{entry.AuthorId}`", true)
                        .AddField("Time", $"<t:{entry.Time}:F>")
                        .AddField("Reason", $"> {entry.Reason}")
                        .SetThumbnail(entry?.AvatarUrl ?? "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/1024px-Steam_icon_logo.svg.png")
                        .SetFooter($"Powered by UCS Collective", "https://ucs.fcosma.it/assets/forum/img/logo.png")
                    )
                );
            }
            catch (Exception e)
            {
                client.Logger.Error(e.ToString());
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

        internal void Close() => client.Disconnect();
    }
}
