using Exiled.API.Enums;
using Exiled.API.Features;
using Handlers = Exiled.Events.Handlers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UncomplicatedDiscordIntegration.API.Features;
using UncomplicatedDiscordIntegration.Events;
using UncomplicatedDiscordIntegration.Manager.NET;
using System.Threading.Tasks;

namespace UncomplicatedDiscordIntegration
{
    internal class Plugin : Plugin<Config, Translation>
    {
        public override string Name => "UncomplicatedDiscordIntegration";

        public override string Prefix => "UncomplicatedDiscordIntegration";

        public override string Author => "FoxWorn3365 & UCS Collective";

        public override Version Version => new(1, 0, 1);

        public override Version RequiredExiledVersion => new(8, 11, 0);

        public override PluginPriority Priority => PluginPriority.High;

        internal static Plugin Instance;

        internal DiscordBot bot;

        private MapHandler mapHandler;

        private ServerHandler serverHandler;

        private PlayerHandler playerHandler;

        private Harmony harmony;

        internal HttpManager httpManager;

        private readonly List<LogMessage> queue = [];

        public override void OnEnabled()
        {
            Instance = this;
            queue.Clear();

            httpManager = new("udi");

            Log.Info("==================================================");
            Log.Info(" Thanks for using UncomplicatedDiscordIntegration");
            Log.Info("         by FoxWorn3365 & UCS Collective");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");


            if (httpManager.LatestVersion.CompareTo(Version) > 0)
                Log.Warn($"You are NOT using the latest version of UncomplicatedDiscordIntegration!\nCurrent: v{Version} | Latest available: v{httpManager.LatestVersion}\nDownload it from GitHub: https://github.com/UncomplicatedCustomServer/UncomplicatedDiscordIntegration/releases/latest");
            else if (httpManager.LatestVersion.CompareTo(Version) < 0)
            {
                Log.Info($"You are using an EXPERIMENTAL or PRE-RELEASE version of UncomplicatedDiscordIntegration!\nLatest stable release: {httpManager.LatestVersion}\nWe do not assure that this version won't make your SCP:SL server crash! - Debug log has been enabled!");
                if (!Log.DebugEnabled.Contains(Assembly))
                {
                    Config.Debug = true;
                    Log.DebugEnabled.Add(Assembly);
                }
            }

            if (Config.Bot.Token == string.Empty)
                Log.Error("Failed to start the bot!\nThe given token is not valid!");
            else
                Task.Run(() => bot = new(Config.Bot.Token));

            WatchlistManager.Init();

            harmony = new($"ucs.udi-{DateTime.Now.Ticks}");
            harmony.PatchAll();

            RegisterEvents();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            harmony.UnpatchAll();

            bot.Close();

            UnregisterEvents();
            base.OnDisabled();
        }

        internal void HandleLogMessage(LogMessage message) => bot.AddMessageToQueue(message);

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

        private void UnregisterEvents()
        {
            Handlers.Map.Decontaminating -= mapHandler.OnDecontaminating;
            Handlers.Warhead.Starting -= mapHandler.OnStartingWarhead;
            Handlers.Warhead.Stopping -= mapHandler.OnStoppingWarhead;
            Handlers.Warhead.Detonated -= mapHandler.OnWarheadDetonated;

            Handlers.Server.WaitingForPlayers -= serverHandler.OnWaitingForPlayers;
            Handlers.Server.RoundStarted -= serverHandler.OnRoundStarted;
            Handlers.Server.RoundEnded -= serverHandler.OnRoundEnded;
            Handlers.Server.RespawningTeam -= serverHandler.OnRespawningTeam;
            Handlers.Server.ReportingCheater -= serverHandler.OnReportingCheater;
            Handlers.Server.LocalReporting -= serverHandler.OnLocalReporting;

            Handlers.Scp914.ChangingKnobSetting -= playerHandler.OnChangingScp914KnobSetting;
            Handlers.Player.UsedItem -= playerHandler.OnUsedMedicalItem;
            Handlers.Scp079.InteractingTesla -= playerHandler.OnInteractingTesla;
            Handlers.Player.PickingUpItem -= playerHandler.OnPickingUpItem;
            Handlers.Player.ActivatingGenerator -= playerHandler.OnInsertingGeneratorTablet;
            Handlers.Player.StoppingGenerator -= playerHandler.OnEjectingGeneratorTablet;
            Handlers.Player.UnlockingGenerator -= playerHandler.OnUnlockingGenerator;
            Handlers.Player.OpeningGenerator -= playerHandler.OnOpeningGenerator;
            Handlers.Player.ClosingGenerator -= playerHandler.OnClosingGenerator;
            Handlers.Scp079.GainingLevel -= playerHandler.OnGainingLevel;
            Handlers.Scp079.GainingExperience -= playerHandler.OnGainingExperience;
            Handlers.Player.EscapingPocketDimension -= playerHandler.OnEscapingPocketDimension;
            Handlers.Player.EnteringPocketDimension -= playerHandler.OnEnteringPocketDimension;
            Handlers.Player.ActivatingWarheadPanel -= playerHandler.OnActivatingWarheadPanel;
            Handlers.Player.TriggeringTesla -= playerHandler.OnTriggeringTesla;
            Handlers.Player.Hurting -= playerHandler.OnHurting;
            Handlers.Player.Dying -= playerHandler.OnDying;
            Handlers.Player.Kicked -= playerHandler.OnKicked;
            Handlers.Player.Banned -= playerHandler.OnBanned;
            Handlers.Player.InteractingDoor -= playerHandler.OnInteractingDoor;
            Handlers.Player.InteractingElevator -= playerHandler.OnInteractingElevator;
            Handlers.Player.InteractingLocker -= playerHandler.OnInteractingLocker;
            Handlers.Player.IntercomSpeaking -= playerHandler.OnIntercomSpeaking;
            Handlers.Player.Handcuffing -= playerHandler.OnHandcuffing;
            Handlers.Player.RemovingHandcuffs -= playerHandler.OnRemovingHandcuffs;
            Handlers.Scp106.Teleporting -= playerHandler.OnTeleporting;
            Handlers.Player.ReloadingWeapon -= playerHandler.OnReloadingWeapon;
            Handlers.Player.DroppingItem -= playerHandler.OnItemDropped;
            Handlers.Player.Verified -= playerHandler.OnVerified;
            Handlers.Player.Destroying -= playerHandler.OnDestroying;
            Handlers.Player.ChangingRole -= playerHandler.OnChangingRole;
            Handlers.Player.ChangingGroup -= playerHandler.OnChangingGroup;
            Handlers.Player.ChangingItem -= playerHandler.OnChangingItem;
            Handlers.Scp914.Activating -= playerHandler.OnActivatingScp914;

            playerHandler = null;
            mapHandler = null;
            serverHandler = null;
        }
    }
}
