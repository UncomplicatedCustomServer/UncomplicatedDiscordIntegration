using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;

using static HarmonyLib.AccessTools;
using UncomplicatedDiscordIntegration.Enums;
using SimpleDiscord.Components.Builders;
using SimpleDiscord.Components;

namespace UncomplicatedDiscordIntegration.Patches
{
    [HarmonyPatch(typeof(Log), nameof(Log.Error), typeof(object))]
    [HarmonyPatch(typeof(Log), nameof(Log.Error), typeof(string))]
    internal class ErrorLoggingPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);

            int offset = -2;
            int index = newInstructions.FindLastIndex(i => i.opcode == OpCodes.Call) + offset;

            newInstructions.InsertRange(index,
            [
                new (OpCodes.Dup),
                new (OpCodes.Call, Method(typeof(ErrorLoggingPatch), nameof(LogError))),
            ]);

            return newInstructions;
        }

        private static void LogError(string message)
        {
            if (Plugin.Instance.Config.LogErrors)
                if (Plugin.Instance.Config.Bot.ErrorWebhookUrl is not null && Plugin.Instance.Config.Bot.ErrorWebhookUrl != string.Empty)
                    Plugin.Instance.bot.client.SendWebhook(Plugin.Instance.Config.Bot.ErrorWebhookUrl, MessageBuilder.New().SetContent("").AddEmbed(EmbedBuilder.New().SetTitle("Error").SetDescription($"```\n{message.Substring(0, 1990)}\n```").SetColor("#d1372c")));
                else
                    Plugin.Instance.HandleLogMessage(new(ChannelType.Errors, message));
        }
    }
}