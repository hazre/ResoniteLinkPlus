using System.Reflection;
using HarmonyLib;

namespace ResoniteLinkPlus;

[HarmonyPatch]
public class ResoniteLinkHostStartPatch
{
    static MethodBase? TargetMethod()
    {
        try
        {
            return LinkPlus.HostRef.Start;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Exception in TargetMethod: {ex}");
            return null;
        }
    }

    static bool Prefix(object __instance, ref int? port)
    {
        try
        {
            if (!Plugin.Enabled.Value) return true;

            string worldName = LinkPlus.GetWorldName(__instance);

            if (port == null)
            {
                int startingPort = Plugin.LinkStartingPort.Value;
                int availablePort = LinkPlus.GetAvailablePort(startingPort);
                port = availablePort;

                Plugin.Log.LogInfo($"World '{worldName}': No port specified. Assigned available port: {availablePort}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogDebug($"Exception in Prefix: {ex}");
            return true;
        }
    }

    static void Postfix(object __instance, int? port, bool __result)
    {
        try
        {
            string worldName = LinkPlus.GetWorldName(__instance);

            if (__result)
            {
                if (LinkPlus.HostRef.Port != null)
                {
                    int actualPort = (int)LinkPlus.HostRef.Port.GetValue(__instance)!;
                    Plugin.Log.LogInfo($"World '{worldName}': ResoniteLink port set to: {actualPort}");
                }
            }
            else
            {
                Plugin.Log.LogWarning($"World '{worldName}': failed to set ResoniteLink port to: {port}");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Exception in Postfix: {ex}");
        }
    }
}