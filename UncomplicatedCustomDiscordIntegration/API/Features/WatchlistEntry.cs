using Exiled.API.Features;
using Newtonsoft.Json;
using SimpleDiscord.Components;
using System;
using UncomplicatedCustomDiscordIntegration.Manager.NET;

namespace UncomplicatedCustomDiscordIntegration.API.Features
{
#nullable enable
    internal class WatchlistEntry
    {
        [JsonProperty("author")]
        public string Author { get; }

        [JsonProperty("author_id")]
        // Can be both @steam and @discord
        public string AuthorId { get; }

        [JsonProperty("target")]
        public string Target { get; }

        [JsonProperty("target_id")]
        // Can be only @steam
        public string TargetId { get; }

        [JsonProperty("time")]
        public long Time { get; }

        [JsonProperty("reason")]
        public string Reason { get; }

        [JsonProperty("avatar_url")]
        public string? AvatarUrl { get; }

        [JsonConstructor]
        public WatchlistEntry(string author, string authorId, string target, string targetId, long time, string reason, string avatarUrl)
        {
            Author = author;
            AuthorId = authorId;
            Target = target;
            TargetId = targetId;
            Time = time;
            Reason = reason;
            AvatarUrl = avatarUrl;
        }

        public WatchlistEntry(Player author, Player target, string reason)
        {
            Author = author.Nickname;
            AuthorId = author.UserId;
            Target = target.Nickname;
            TargetId = target.UserId;
            Reason = reason;
            AvatarUrl = WatchlistManager.Sync(Plugin.Instance.httpManager.TryGetNickname(target.UserId.Replace("@steam", ""))).Split('@')[0];
            Time = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static bool TryCreateFromDiscord(SocketUser author, string target, string reason, out WatchlistEntry? entry)
        {
            string targetNameAndAvatar = WatchlistManager.Sync(Plugin.Instance.httpManager.TryGetNickname(target.Replace("@steam", "")));
            //string targetName = "Pizzacat100";

            if (targetNameAndAvatar != string.Empty && targetNameAndAvatar is not null)
            {
                string[] targets = targetNameAndAvatar.Split('@');
                entry = new(author.Username, $"{author.Id}@discord", targets[0], target.Contains("@steam") ? target : $"{target}@steam", DateTimeOffset.Now.ToUnixTimeSeconds(), reason, targets[1]);
                return true;
            }

            entry = null;
            return false;
        }

        public static bool TryCreateFromOffline(Player author, string target, string reason, out WatchlistEntry? entry)
        {
            string targetNameAndAvatar = WatchlistManager.Sync(Plugin.Instance.httpManager.TryGetNickname(target.Replace("@steam", "")));
            //string targetName = "Pizzacat100";

            if (targetNameAndAvatar != string.Empty && targetNameAndAvatar is not null)
            {
                string[] targets = targetNameAndAvatar.Split('@');
                entry = new(author.Nickname, author.UserId, targets[0], target.Contains("@steam") ? target : $"{target}@steam", DateTimeOffset.Now.ToUnixTimeSeconds(), reason, targets[1]);
                return true;
            }

            entry = null;
            return false;
        }
    }
}
