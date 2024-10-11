using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json.Converters;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UncomplicatedDiscordIntegration.API.Features;
using UncomplicatedDiscordIntegration.Manager.NET;

namespace UncomplicatedDiscordIntegration.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Watchlist : ParentCommand
    {
        public Watchlist() => LoadGeneratedCommands();

        public override string Command { get; } = "watchlist";

        public override string[] Aliases { get; } = ["wl"];

        public override string Description { get; } = "Manage the watchlist";

        public override void LoadGeneratedCommands() { }

#nullable enable
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Task<KeyValuePair<bool, string>> answer = Task.Run(() => Execute(arguments, sender));
            answer.Wait();
            response = answer.Result.Value;
            return answer.Result.Key;
        }

        public static async Task<KeyValuePair<bool, string>> Execute(ArraySegment<string> arguments, ICommandSender sender)
        {
            string response;

            if (arguments.Count == 0 && sender.CheckPermission("udi.watchlist.help"))
            {
                response = $"UncomplicatedDiscordIntegration v{Plugin.Instance.Version} - Watchlist\n\nSubcommands:\n- watchlist list\n- watchlist add <player name / Id / steamid> <reason>\n- watchlist remove <player name / Id / steamid>\n- watchlist view <player name / Id / steamid>\n- watchlist clear";
                return new(true, response);
            }

            string target;
            string steamId;

            switch (arguments.At(0))
            {
                case "list":
                    response = $"UncomplicatedDiscordIntegration v{Plugin.Instance.Version} - Watchlist entries\n";
                    
                    Log.Warn("PORCO IL CRISTO TROIA [1]");
                    WatchlistEntry[]? entries = await WatchlistManager.Get();
                    Log.Warn("PORCO IL CRISTO TROIA [2]");

                    if (entries is not null)
                        foreach (WatchlistEntry entryi in entries)
                            response += $"\n{entryi.Target} ({entryi.TargetId})\n    {DateTimeOffset.FromUnixTimeSeconds(entryi.Time).ToLocalTime()} by {entryi.Target} ({entryi.TargetId})\n    {entryi.Reason}";

                    return new(true, response);
                case "add":
                    Player author = Player.Get(sender);

                    if (author is null)
                    {
                        response = $"Can't use this command without being inside the game!";
                        return new(false, response);
                    }

                    if (arguments.Count < 3)
                    {
                        response = $"Failed to add an user to the watchlist!\nUsage: watchlist add <player name / Id / steamid> <reason>";
                        return new(false, response);
                    }

                    target = arguments.At(1);
                    string reason = GetSection(arguments.Array, 2);

                    WatchlistEntry? entry = null;

                    if (target.Contains("@steam") || target.Length == 17)
                        WatchlistEntry.TryCreateFromOffline(author, target, reason, out entry);
                    else
                    {
                        Player player = Player.Get(target);
                        if (player is not null)
                            entry = new(author, player, reason);
                    }

                    if (entry is null)
                    {
                        response = $"Player not found!\nInput: {target}";
                        return new(false, response);
                    }

                    if (await WatchlistManager.Add(entry))
                    {
                        response = $"Player {entry.Target} successfully added to the watchlist!";
                        return new(true, response);
                    }

                    response = $"Failed to add player {entry.Target} to the watchlist!\nCommon reasons are:\n- the player is already inside the watchlist\n- the server can't reach the remote database";
                    return new(false, response);
                case "remove":
                    target = arguments.At(1);

                    if (target.Contains("@steam") || target.Length == 17)
                        steamId = target.Contains("@steam") ? target : $"{target}@steam";
                    else
                    {
                        Player playerTarget = Player.Get(target);
                        steamId = playerTarget.UserId;
                    }

                    if (await WatchlistManager.Remove(steamId))
                    {
                        response = $"Player {steamId} successfully removed from the watchlist!";
                        return new(true, response);
                    }

                    response = $"Failed to add player {steamId} to the watchlist!\nCommon reasons are:\n- the player is not inside the watchlist\n- the server can't reach the remote database";
                    return new(false, response);
                case "view":
                    target = arguments.At(1);

                    if (target.Contains("@steam") || target.Length == 17)
                        steamId = target.Contains("@steam") ? target : $"{target}@steam";
                    else
                    {
                        Player playerTarget = Player.Get(target);
                        steamId = playerTarget.UserId;
                    }

                    WatchlistEntry? targetEntry = await WatchlistManager.Get(steamId);

                    if (targetEntry is null)
                    {
                        response = $"Player {steamId} is not present inside the watchlist!";
                        return new(false, response);
                    }

                    response = $"UncomplicatedDiscordIntegration v{Plugin.Instance.Version} - Watchlist\n\nPlayer:         {targetEntry.Target} ({targetEntry.TargetId})\nWatchlisted by: {targetEntry.Author} ({targetEntry.AuthorId})\nAt:             {DateTimeOffset.FromUnixTimeSeconds(targetEntry.Time).ToLocalTime()}\nReason:\n{targetEntry.Reason}";
                    return new(false, response);
                case "clear":
                    await WatchlistManager.Clear();
                    response = "Successfully cleared the watchlist!";
                    return new(true, response);
                default:
                    response = $"Subcommand {arguments.At(0)} not found!";
                    return new(false, response);
            }
        }

        public static string GetSection(string[] array, int from)
        {
            if (array.Length >= from)
                return string.Empty;

            List<string> output = [];
            for (int a = from; a < array.Length; a++)
                output.Add(array.ElementAt(a));

            return string.Join(" ", output);
        }
    }
}
