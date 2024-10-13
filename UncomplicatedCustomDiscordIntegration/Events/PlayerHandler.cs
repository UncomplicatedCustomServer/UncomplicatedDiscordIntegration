// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp079;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Permissions.Extensions;
using PlayerRoles;
using System;
using System.Linq;
using UncomplicatedDiscordIntegration;
using UncomplicatedDiscordIntegration.API.Features;
using UncomplicatedDiscordIntegration.Enums;
using UncomplicatedDiscordIntegration.Manager.NET;

namespace UncomplicatedDiscordIntegration.Events
{
    /// <summary>
    /// Handles player-related events.
    /// </summary>
    internal sealed class PlayerHandler
    {
#pragma warning disable SA1600 // Elements should be documented
        public void OnInsertingGeneratorTablet(ActivatingGeneratorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerInsertingGeneratorTablet && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorInserted, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerInsertingGeneratorTablet)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GeneratorInserted, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnOpeningGenerator(OpeningGeneratorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerOpeningGenerator && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorOpened, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerOpeningGenerator)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GeneratorOpened, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerUnlockingGenerator && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorUnlocked, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerUnlockingGenerator)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GeneratorUnlocked, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ChangingPlayerItem && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.ItemChanged, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.CurrentItem.Type, ev.Item.Type)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.ChangingPlayerItem)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.ItemChanged, ev.Player.Nickname, ev.Player.UserId, ev.Player.CurrentItem.Type, ev.Item.Type)));
        }

        public void OnGainingExperience(GainingExperienceEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.GainingScp079Experience && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GainedExperience, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Amount, ev.GainType)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.GainingScp079Experience)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GainedExperience, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Amount, ev.GainType)));
        }

        public void OnGainingLevel(GainingLevelEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.GainingScp079Level && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
#pragma warning disable CS0618 // Type or member is obsolete
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GainedLevel, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.NewLevel - 1, ev.NewLevel)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.GainingScp079Level)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GainedLevel, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.NewLevel - 1, ev.NewLevel)));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void OnDestroying(DestroyingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerLeft && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.LeftServer, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerLeft)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.LeftServer, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnReloadingWeapon(ReloadingWeaponEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ReloadingPlayerWeapon && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.Reloaded, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.CurrentItem.Type, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.ReloadingPlayerWeapon)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.Reloaded, ev.Player.Nickname, ev.Player.UserId, ev.Player.CurrentItem.Type, ev.Player.Role)));
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerActivatingWarheadPanel && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.AccessedWarhead, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerActivatingWarheadPanel)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.AccessedWarhead, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnInteractingElevator(InteractingElevatorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerInteractingElevator && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.CalledElevator, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerInteractingElevator)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.CalledElevator, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerInteractingLocker && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.UsedLocker, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerInteractingLocker)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.UsedLocker, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerTriggeringTesla && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasTriggeredATeslaGate, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerTriggeringTesla)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasTriggeredATeslaGate, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnClosingGenerator(ClosingGeneratorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerClosingGenerator && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorClosed, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerClosingGenerator)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GeneratorClosed, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnEjectingGeneratorTablet(StoppingGeneratorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerEjectingGeneratorTablet && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GeneratorEjected, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerEjectingGeneratorTablet)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GeneratorEjected, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerInteractingDoor && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(ev.Door.IsOpen ? Plugin.Instance.Translation.HasClosedADoor : Plugin.Instance.Translation.HasOpenedADoor, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Door.Nametag)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerInteractingDoor)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(ev.Door.IsOpen ? Plugin.Instance.Translation.HasClosedADoor : Plugin.Instance.Translation.HasOpenedADoor, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Door.Nametag)));
        }

        public void OnActivatingScp914(ActivatingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ActivatingScp914 && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.Scp914HasBeenActivated, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, Exiled.API.Features.Scp914.KnobStatus)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.ActivatingScp914)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.Scp914HasBeenActivated, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, Exiled.API.Features.Scp914.KnobStatus)));
        }

        public void OnChangingScp914KnobSetting(ChangingKnobSettingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.ChangingScp914KnobSetting && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.Scp914KnobSettingChanged, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.KnobSetting)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.ChangingScp914KnobSetting)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.Scp914KnobSettingChanged, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.KnobSetting)));
        }

        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerEnteringPocketDimension && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasEnteredPocketDimension, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerEnteringPocketDimension)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasEnteredPocketDimension, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerEscapingPocketDimension && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasEscapedPocketDimension, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerEscapingPocketDimension)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasEscapedPocketDimension, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnTeleporting(TeleportingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.Scp106Teleporting && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.Scp106Teleported, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.Scp106Teleporting)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.Scp106Teleported, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnInteractingTesla(InteractingTeslaEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.Scp079InteractingTesla && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasTriggeredATeslaGate, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.Scp079InteractingTesla)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasTriggeredATeslaGate, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnHurting(HurtingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.HurtingPlayer && ev.Player != null && (ev.Attacker == null || !Plugin.Instance.Config.ShouldLogFriendlyFireDamageOnly || ev.Attacker.Role.Side == ev.Player.Role.Side) && (!Plugin.Instance.Config.ShouldRespectDoNotTrack || (ev.Attacker == null || (!ev.Attacker.DoNotTrack && !ev.Player.DoNotTrack))) && !Plugin.Instance.Config.BlacklistedDamageTypes.Contains(ev.DamageHandler.Type) && (!Plugin.Instance.Config.OnlyLogPlayerDamage || ev.Attacker != null))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasDamagedForWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", Plugin.Instance.Config.ShouldLogUserIds ? ev.Attacker != null ? ev.Attacker.UserId : string.Empty : Plugin.Instance.Translation.Redacted, ev.Attacker?.Role ?? RoleTypeId.None, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Amount, ev.DamageHandler.Type)));

            if (Plugin.Instance.Config.StaffOnlyEventsToLog.HurtingPlayer && ev.Player != null && (ev.Attacker == null || !Plugin.Instance.Config.ShouldLogFriendlyFireDamageOnly || ev.Attacker.Role.Side == ev.Player.Role.Side) && !Plugin.Instance.Config.BlacklistedDamageTypes.Contains(ev.DamageHandler.Type) && (!Plugin.Instance.Config.OnlyLogPlayerDamage || ev.Attacker != null))
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasDamagedForWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", ev.Attacker != null ? ev.Attacker.UserId : string.Empty, ev.Attacker?.Role ?? RoleTypeId.None, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Amount, ev.DamageHandler.Type)));
        }

        public void OnDying(DyingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerDying && ev.Player != null && (ev.Attacker == null || !Plugin.Instance.Config.ShouldLogFriendlyFireKillsOnly || ev.Attacker.Role.Side == ev.Player.Role.Side) && (!Plugin.Instance.Config.ShouldRespectDoNotTrack || (ev.Attacker == null || (!ev.Attacker.DoNotTrack && !ev.Player.DoNotTrack))))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasKilledWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", Plugin.Instance.Config.ShouldLogUserIds ? ev.Attacker != null ? ev.Attacker.UserId : string.Empty : Plugin.Instance.Translation.Redacted, ev.Attacker?.Role ?? RoleTypeId.None, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.DamageHandler.Type)));

            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerDying && ev.Attacker != null && (ev.Attacker == null || !Plugin.Instance.Config.ShouldLogFriendlyFireKillsOnly || ev.Attacker.Role.Side == ev.Player.Role.Side))
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasKilledWith, ev.Attacker != null ? ev.Attacker.Nickname : "Server", ev.Attacker != null ? ev.Attacker.UserId : string.Empty, ev.Attacker?.Role ?? RoleTypeId.None, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.DamageHandler.Type)));
        }

        public void OnThrowingGrenade(ThrowingRequestEventArgs ev)
        {
            if (ev.Player != null && Plugin.Instance.Config.EventsToLog.PlayerThrowingGrenade && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.ThrewAGrenade, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Item.Type)));
            if (ev.Player != null && Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerThrowingGrenade)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.ThrewAGrenade, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Item.Type)));
        }

        public void OnUsedMedicalItem(UsedItemEventArgs ev)
        {
            if (ev.Player != null && Plugin.Instance.Config.EventsToLog.PlayerUsedMedicalItem && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.UsedMedicalItem, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Item)));
            if (ev.Player != null && Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerUsedMedicalItem)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.UsedMedicalItem, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Item)));
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player != null && Plugin.Instance.Config.EventsToLog.ChangingPlayerRole && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.ChangedRole, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.NewRole)));
            if (ev.Player != null && Plugin.Instance.Config.StaffOnlyEventsToLog.ChangingPlayerRole)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.ChangedRole, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.NewRole)));
        }

        public void OnVerified(VerifiedEventArgs ev)
        {
            if (Plugin.Instance.Config.UseWatchlist)
            {
                WatchlistEntry entry = WatchlistManager.GetSync(ev.Player.UserId);
                if (entry is not null)
                {
                    Plugin.Instance.HandleLogMessage(new(ChannelType.Watchlist, string.Format(Plugin.Instance.Translation.WatchlistedUserJoined, ev.Player.Nickname, ev.Player.UserId, entry.Reason)));
                    foreach (Player player in Player.List.Where(p => p.CheckPermission("udi.broadcast") || p.RemoteAdminAccess))
                        player.Broadcast(7, string.Format(Plugin.Instance.Translation.WatchlistedUserJoined, ev.Player.Nickname, ev.Player.UserId, entry.Reason));
                }
            }

            if (Plugin.Instance.Config.EventsToLog.PlayerJoined && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasJoinedTheGame, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, Plugin.Instance.Config.ShouldLogIPAddresses ? ev.Player.IPAddress : Plugin.Instance.Translation.Redacted)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerJoined)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasJoinedTheGame, ev.Player.Nickname, ev.Player.UserId, ev.Player.IPAddress)));
        }

        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerRemovingHandcuffs && ((!ev.Player.DoNotTrack && !ev.Target.DoNotTrack) || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasBeenFreedBy, ev.Target.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Target.UserId : Plugin.Instance.Translation.Redacted, ev.Target.Role, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerRemovingHandcuffs)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasBeenFreedBy, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.HandcuffingPlayer && ((!ev.Player.DoNotTrack && !ev.Target.DoNotTrack) || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasBeenHandcuffedBy, ev.Target.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Target.UserId : Plugin.Instance.Translation.Redacted, ev.Target.Role, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.HandcuffingPlayer)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasBeenHandcuffedBy, ev.Target.Nickname, ev.Target.UserId, ev.Target.Role, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnKicked(KickedEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerBanned)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Bans, string.Format(Plugin.Instance.Translation.WasKicked, ev.Player?.Nickname ?? Plugin.Instance.Translation.NotAuthenticated, ev.Player?.UserId ?? Plugin.Instance.Translation.NotAuthenticated, ev.Reason)));
        }

        public void OnBanned(BannedEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerBanned)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Bans, string.Format(Plugin.Instance.Translation.WasBannedBy, ev.Details.OriginalName, ev.Details.Id, ev.Details.Issuer, ev.Details.Reason, new DateTime(ev.Details.Expires).ToString(Plugin.Instance.Config.DateFormat))));
        }

        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.Player != null && Plugin.Instance.Config.EventsToLog.PlayerIntercomSpeaking && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasStartedUsingTheIntercom, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role)));
            if (ev.Player != null && Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerIntercomSpeaking)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasStartedUsingTheIntercom, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role)));
        }

        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerPickingupItem && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasPickedUp, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Pickup.Type)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerPickingupItem)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasPickedUp, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Pickup.Type)));
        }

        public void OnItemDropped(DroppingItemEventArgs ev)
        {
            if (Plugin.Instance.Config.EventsToLog.PlayerItemDropped && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.HasDropped, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.Item.Type)));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.PlayerItemDropped)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.HasDropped, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.Item.Type)));
        }

        public void OnChangingGroup(ChangingGroupEventArgs ev)
        {
            if (ev.Player != null && Plugin.Instance.Config.EventsToLog.ChangingPlayerGroup && (!ev.Player.DoNotTrack || !Plugin.Instance.Config.ShouldRespectDoNotTrack))
                Plugin.Instance.HandleLogMessage(new(ChannelType.GameEvents, string.Format(Plugin.Instance.Translation.GroupSet, ev.Player.Nickname, Plugin.Instance.Config.ShouldLogUserIds ? ev.Player.UserId : Plugin.Instance.Translation.Redacted, ev.Player.Role, ev.NewGroup?.BadgeText ?? Plugin.Instance.Translation.None, ev.NewGroup?.BadgeColor ?? Plugin.Instance.Translation.None)));
            if (ev.Player != null && Plugin.Instance.Config.StaffOnlyEventsToLog.ChangingPlayerGroup)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.GroupSet, ev.Player.Nickname, ev.Player.UserId, ev.Player.Role, ev.NewGroup?.BadgeText ?? Plugin.Instance.Translation.None, ev.NewGroup?.BadgeColor ?? Plugin.Instance.Translation.None)));
        }
    }
}