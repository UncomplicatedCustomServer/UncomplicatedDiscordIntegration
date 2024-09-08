using Exiled.API.Features;
using System.Collections.Generic;
using System.IO;

namespace UncomplicatedCustomDiscordIntegration.API.Features
{
    internal class Watchlist
    {
        public static readonly Dictionary<string, string> Users = [];

        public static readonly string FilePath = Path.Combine(Paths.Configs, ".ucdiwatchlist.txt");

        public static void Init()
        {
            Users.Clear();
            if (File.Exists(FilePath))
                foreach (string player in File.ReadAllText(FilePath).Split('|'))
                    Users.Add(player.Split(' ')[0], player.Replace($"{player.Split(' ')[0]} ", string.Empty));
        }

        private static void Update()
        {
            List<string> data = [];
            foreach (KeyValuePair<string, string> pair in Users)
                data.Add($"{pair.Key} {pair.Value}");
            File.WriteAllText(FilePath, string.Join("|", data));
        }

        public static void Add(Player player, string reason)
        {
            if (Users.ContainsKey(player.UserId))
                return;

            Users.Add(player.UserId, reason.Replace("|", "<altupline>"));

        }
    }
}
