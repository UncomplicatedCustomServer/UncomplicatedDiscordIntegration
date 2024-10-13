using Exiled.API.Features;
using HarmonyLib;
using RemoteAdmin;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System;
using UncomplicatedDiscordIntegration.Enums;

using static HarmonyLib.AccessTools;

namespace UncomplicatedDiscordIntegration.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class CommandLogging
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            const int index = 0;

            newInstructions.InsertRange(index,
            [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, Method(typeof(CommandLogging), nameof(LogCommand))),
            ]);

            return newInstructions;
        }

        private static void LogCommand(string query, CommandSender sender)
        {
            if (!Plugin.Instance.Config.EventsToLog.SendingRemoteAdminCommands && !Plugin.Instance.Config.StaffOnlyEventsToLog.SendingRemoteAdminCommands)
                return;

            string[] args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
            if (args[0].StartsWith("$"))
                return;

            Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;
            if (player == null || (!string.IsNullOrEmpty(player.UserId) && Plugin.Instance.Config.TrustedAdmins.Contains(player.UserId)))
                return;
            if (Plugin.Instance.Config.EventsToLog.SendingRemoteAdminCommands)
                Plugin.Instance.HandleLogMessage(new(ChannelType.Command, string.Format(Plugin.Instance.Translation.UsedCommand, sender.Nickname, sender.SenderId ?? Plugin.Instance.Translation.DedicatedServer, player.Role, args[0], string.Join(" ", args.Where(a => a != args[0])))));
            if (Plugin.Instance.Config.StaffOnlyEventsToLog.SendingRemoteAdminCommands)
                Plugin.Instance.HandleLogMessage(new(ChannelType.StaffCopy, string.Format(Plugin.Instance.Translation.UsedCommand, sender.Nickname, sender.SenderId ?? Plugin.Instance.Translation.DedicatedServer, player.Role, args[0], string.Join(" ", args.Where(a => a != args[0])))));
        }
    }
}