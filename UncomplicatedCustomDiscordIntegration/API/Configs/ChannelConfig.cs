namespace UncomplicatedDiscordIntegration.API.Configs
{
    public sealed class ChannelConfig
    {
        public long Commands { get; set; }

        public long GameEvents { get; set; }

        public long Bans { get; set; }

        public long Reports { get; set; }

        public long StaffChannel { get; set; }

        public long Errors { get; set; }

        public long Watchlist { get; set; }
    }
}
