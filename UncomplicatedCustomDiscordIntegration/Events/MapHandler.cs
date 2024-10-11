using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomItems.API.EventArgs;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.EventArgs.Warhead;
using System.Linq;
using UncomplicatedDiscordIntegration;
using UncomplicatedDiscordIntegration.Enums;

namespace UncomplicatedDiscordIntegration.Events
{
    internal sealed class MapHandler
    {
        public void OnWarheadDetonated()
        {
            if (Plugin.Instance.Config.EventsToLog.WarheadDetonated)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, Plugin.Instance.Translation.WarheadHasDetonated));
        }

        public void OnGeneratorActivated(GeneratorActivatingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.GeneratorActivated)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorFinished, ev.Generator.Room, Generator.Get(GeneratorState.Engaged).Count() + 1)));
        }

        public void OnDecontaminating(DecontaminatingEventArgs _)
        {
            if (Plugin.Instance.Config.EventsToLog.Decontaminating)
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, Plugin.Instance.Translation.DecontaminationHasBegun));
        }

        public void OnStartingWarhead(StartingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.StartingWarhead && (ev.Player == null || (ev.Player != null && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))))
            {
                object[] vars = ev.Player == null ? [Warhead.DetonationTimer] : [ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, Warhead.DetonationTimer];

                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(ev.Player == null ? Plugin.Instance.Translation.WarheadStarted : Plugin.Instance.Translation.PlayerWarheadStarted, vars)));
            }
        }

        public void OnStoppingWarhead(StoppingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.StoppingWarhead && (ev.Player == null || (ev.Player != null && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))))
            {
                object[] vars = ev.Player == null ? [] : [ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role];

                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(ev.Player == null ? Plugin.Instance.Translation.CanceledWarhead : Plugin.Instance.Translation.PlayerCanceledWarhead, vars)));
            }

            if (Plugin.Instance.Config.StaffOnlyEventsToLog.StoppingWarhead)
            {
                object[] vars = ev.Player == null ? [] : [ev.Player.Nickname, ev.Player.UserId, ev.Player.Role];

                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(ev.Player == null ? Plugin.Instance.Translation.CanceledWarhead : Plugin.Instance.Translation.PlayerCanceledWarhead, vars)));
            }
        }

        public void OnUpgradingItems(UpgradingPickupEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.UpgradingScp914Items)
            {
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.Scp914ProcessedItem, ev.Pickup.Type)));
            }
        }
    }
}