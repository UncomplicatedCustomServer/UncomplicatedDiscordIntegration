using RemoteAdmin;
using Exiled.API.Features;
using HarmonyLib;
using UncomplicatedCustomDiscordIntegration;
using UncomplicatedCustomDiscordIntegration.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System;

using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomDiscordIntegration.Patches
{
    [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
    internal class ClientCommandLogging
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instruction)
        {
            List<CodeInstruction> newInstructions = new(instruction);
            const int index = 0;

            newInstructions.InsertRange(index,
            [
                new (OpCodes.Ldarg_1),
                new (OpCodes.Ldarg_0),
                new (OpCodes.Ldfld, Field(typeof(QueryProcessor), nameof(QueryProcessor._sender))),
                new (OpCodes.Call, Method(typeof(ClientCommandLogging), nameof(LogCommand))),
            ]);

            return newInstructions;
        }

        private static void LogCommand(string query, CommandSender sender)
        {
            if (!Plugin.Instance.Config.EventsToLog.SendingConsoleCommands && !Plugin.Instance.Config.StaffOnlyEventsToLog.SendingConsoleCommands)
                return;

            string[] args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
            if (args[0].StartsWith("$"))
                return;

            Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;
            if (player == null || (!string.IsNullOrEmpty(player.UserId) && Plugin.Instance.Config.TrustedAdmins.Contains(player.UserId)))
                return;
            if (Plugin.Instance.Config.EventsToLog.SendingConsoleCommands)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Command, string.Format(Plugin.Instance.Translation.UsedCommand, sender.Nickname, sender.SenderId ?? Plugin.Instance.Translation.DedicatedServer, player.Role, args[0], string.Join(" ", args.Where(a => a != args[0])))));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.SendingConsoleCommands)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.UsedCommand, sender.Nickname, sender.SenderId ?? Plugin.Instance.Translation.DedicatedServer, player.Role, args[0], string.Join(" ", args.Where(a => a != args[0])))));
        }
    }
}