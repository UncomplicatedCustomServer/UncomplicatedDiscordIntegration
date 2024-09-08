using Exiled.API.Enums;
using Exiled.API.Features;
using Handlers = Exiled.Events.Handlers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UncomplicatedCustomDiscordIntegration.API.Features;
using UncomplicatedDiscordIntegration.Events;

namespace UncomplicatedCustomDiscordIntegration
{
    internal class Plugin : Plugin<Config, Translation>
    {
        public override string Name => "UncomplicatedCustomDiscordIntegration";

        public override string Prefix => "UncomplicatedCustomDiscordIntegration";

        public override string Author => "FoxWorn3365 & UCS Collective";

        public override Version Version => new(0, 9, 0);

        public override Version RequiredExiledVersion => new(8, 11, 0);

        public override PluginPriority Priority => PluginPriority.High;

        internal static Plugin Instance;

        private DiscordBot bot;

        private MapHandler mapHandler;

        private ServerHandler serverHandler;

        private PlayerHandler playerHandler;

        private Harmony harmony;

        private readonly List<LogMessage> queue = [];

        public override void OnEnabled()
        {
            Instance = this;
            queue.Clear();

            if (Config.Bot.Token == string.Empty)
                Log.Error("Failed to start the bot!\nThe given token is not valid!");
            else
                bot = new(Config.Bot.Token);

            Watchlist.Init();

            harmony = new($"ucs.ucdi-{DateTime.Now.Ticks}");
            harmony.PatchAll();

            RegisterEvents();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            bot.Close();

            base.OnDisabled();
        }

        internal void HandleLogMessage(LogMessage message)
        {
            if (queue.Count > 0 && bot.channels.Count == 7)
                foreach (LogMessage msg in queue)
                    HandleLogMessage(msg);
                
            if (bot.channels.Count < 7)
            {
                queue.Add(message);
                return;
            }

            bot.AddMessageToQueue(message);
        }

        private void RegisterEvents()
        {
            mapHandler = new MapHandler();
            serverHandler = new ServerHandler();
            playerHandler = new PlayerHandler();

            Handlers.Map.Decontaminating += mapHandler.OnDecontaminating;
            Handlers.Map.GeneratorActivating += mapHandler.OnGeneratorActivated;
            Handlers.Warhead.Starting += mapHandler.OnStartingWarhead;
            Handlers.Warhead.Stopping += mapHandler.OnStoppingWarhead;
            Handlers.Warhead.Detonated += mapHandler.OnWarheadDetonated;
            Handlers.Scp914.UpgradingPickup += mapHandler.OnUpgradingItems;

            Handlers.Server.WaitingForPlayers += serverHandler.OnWaitingForPlayers;
            Handlers.Server.RoundStarted += serverHandler.OnRoundStarted;
            Handlers.Server.RoundEnded += serverHandler.OnRoundEnded;
            Handlers.Server.RespawningTeam += serverHandler.OnRespawningTeam;
            Handlers.Server.ReportingCheater += serverHandler.OnReportingCheater;
            Handlers.Server.LocalReporting += serverHandler.OnLocalReporting;

            Handlers.Scp914.ChangingKnobSetting += playerHandler.OnChangingScp914KnobSetting;
            Handlers.Player.UsedItem += playerHandler.OnUsedMedicalItem;
            Handlers.Scp079.InteractingTesla += playerHandler.OnInteractingTesla;
            Handlers.Player.PickingUpItem += playerHandler.OnPickingUpItem;
            Handlers.Player.ActivatingGenerator += playerHandler.OnInsertingGeneratorTablet;
            Handlers.Player.StoppingGenerator += playerHandler.OnEjectingGeneratorTablet;
            Handlers.Player.UnlockingGenerator += playerHandler.OnUnlockingGenerator;
            Handlers.Player.OpeningGenerator += playerHandler.OnOpeningGenerator;
            Handlers.Player.ClosingGenerator += playerHandler.OnClosingGenerator;
            Handlers.Scp079.GainingLevel += playerHandler.OnGainingLevel;
            Handlers.Scp079.GainingExperience += playerHandler.OnGainingExperience;
            Handlers.Player.EscapingPocketDimension += playerHandler.OnEscapingPocketDimension;
            Handlers.Player.EnteringPocketDimension += playerHandler.OnEnteringPocketDimension;
            Handlers.Player.ActivatingWarheadPanel += playerHandler.OnActivatingWarheadPanel;
            Handlers.Player.TriggeringTesla += playerHandler.OnTriggeringTesla;
            Handlers.Player.ThrowingRequest += playerHandler.OnThrowingGrenade;
            Handlers.Player.Hurting += playerHandler.OnHurting;
            Handlers.Player.Dying += playerHandler.OnDying;
            Handlers.Player.Kicked += playerHandler.OnKicked;
            Handlers.Player.Banned += playerHandler.OnBanned;
            Handlers.Player.InteractingDoor += playerHandler.OnInteractingDoor;
            Handlers.Player.InteractingElevator += playerHandler.OnInteractingElevator;
            Handlers.Player.InteractingLocker += playerHandler.OnInteractingLocker;
            Handlers.Player.IntercomSpeaking += playerHandler.OnIntercomSpeaking;
            Handlers.Player.Handcuffing += playerHandler.OnHandcuffing;
            Handlers.Player.RemovingHandcuffs += playerHandler.OnRemovingHandcuffs;
            Handlers.Scp106.Teleporting += playerHandler.OnTeleporting;
            Handlers.Player.ReloadingWeapon += playerHandler.OnReloadingWeapon;
            Handlers.Player.DroppingItem += playerHandler.OnItemDropped;
            Handlers.Player.Verified += playerHandler.OnVerified;
            Handlers.Player.Destroying += playerHandler.OnDestroying;
            Handlers.Player.ChangingRole += playerHandler.OnChangingRole;
            Handlers.Player.ChangingGroup += playerHandler.OnChangingGroup;
            Handlers.Player.ChangingItem += playerHandler.OnChangingItem;
            Handlers.Scp914.Activating += playerHandler.OnActivatingScp914;
        }
    }
}
