using HarmonyLib;

namespace UncomplicatedDiscordIntegration.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal class ServerNamePatch
    {
        private static void Postfix() => ServerConsole._serverName += $"<color=#00000000><size=1>UDI 1.0.0</size></color>";
    }
}
