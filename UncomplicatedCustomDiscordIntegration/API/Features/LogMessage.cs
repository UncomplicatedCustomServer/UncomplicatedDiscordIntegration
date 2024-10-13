using System;
using UncomplicatedDiscordIntegration.Enums;

namespace UncomplicatedDiscordIntegration.API.Features
{
    internal class LogMessage(ChannelType channel, string log)
    {
        public ChannelType ChannelType { get; } = channel;

        public string Log { get; } = log;

        public string Time { get; } = DateTimeOffset.Now.ToLocalTime().ToString();

        public override string ToString() => $"[{Time}] [{ChannelType}] {Log}";
    }
}