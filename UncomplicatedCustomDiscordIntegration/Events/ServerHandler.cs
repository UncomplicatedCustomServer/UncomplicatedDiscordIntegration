using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Respawning;
using UncomplicatedCustomDiscordIntegration;
using UncomplicatedCustomDiscordIntegration.Enums;

namespace UncomplicatedDiscordIntegration.Events
{
    internal sealed class ServerHandler
    {
        public void OnReportingCheater(ReportingCheaterEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ReportingCheater)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Reports, string.Format(Plugin.Instance.Translation.CheaterReportFilled, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Reason)));
        }

        public void OnLocalReporting(LocalReportingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ReportingCheater)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Reports, string.Format(Plugin.Instance.Translation.CheaterReportFilled, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Reason)));
        }

        public void OnWaitingForPlayers()
        {
            if (Plugin.Instance.Config.EventsToLog.WaitingForPlayers)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, Plugin.Instance.Translation.WaitingForPlayers));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.WaitingForPlayers)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, Plugin.Instance.Translation.WaitingForPlayers));
        }

        public void OnRoundStarted()
        {
            if (Plugin.Instance.Config.EventsToLog.RoundStarted)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.RoundStarting, Player.List.Count)));
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.RoundEnded)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.RoundEnded, ev.LeadingTeam, Player.List.Count, Server.MaxPlayerCount)));
        }

        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.RespawningTeam)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency ? Plugin.Instance.Translation.ChaosInsurgencyHaveSpawned : Plugin.Instance.Translation.NineTailedFoxHaveSpawned, ev.Players.Count)));
        }
    }
}