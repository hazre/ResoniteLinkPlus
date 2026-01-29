using FrooxEngine;
using HarmonyLib;
using System.Reflection;

namespace ResoniteLinkPlus;

[HarmonyPatch(typeof(World), "StartSession")]
public class WorldStartSessionPatch
{
    private static readonly MethodInfo? _startResoniteLinkMethod = AccessTools.Method(typeof(World), "StartResoniteLink", [typeof(int?)]);

    static void Postfix(World __result)
    {
        if (!Plugin.Enabled.Value || __result == null) return;

        var (shouldStart, port) = LinkPlus.AutoStartInfo;
        LinkPlus.AutoStartInfo = (false, null);

        if (!shouldStart) return;

        __result.RunInUpdates(3, () =>
        {
            if (__result.IsDestroyed) return;

            try
            {
                if (_startResoniteLinkMethod == null)
                {
                    Plugin.Log.LogError("Could not find StartResoniteLink method on World");
                    return;
                }

                _startResoniteLinkMethod.Invoke(__result, [port]);
                // TODO: Get actual port from ResoniteLinkHost instance, "auto" is pointless
                Plugin.Log.LogInfo($"Started ResoniteLink for world '{__result.Name}' with port: {port?.ToString() ?? "auto"}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Failed to start ResoniteLink: {ex}");
            }
        });
    }
}

[HarmonyPatch(typeof(World), "Destroy")]
public class WorldDestroyPatch
{
    static void Prefix(World __instance)
    {
        if (!Plugin.Enabled.Value || __instance == null) return;

        try
        {
            if (LinkPlus.WorldRef.ResoniteLink == null)
            {
                Plugin.Log.LogError("Could not find ResoniteLink property on World");
                return;
            }

            var resoniteLinkHost = LinkPlus.WorldRef.ResoniteLink.GetValue(__instance);
            if (resoniteLinkHost != null)
            {
                var stopMethod = AccessTools.Method(LinkPlus.HostRef.Type!, "Dispose");
                if (stopMethod != null)
                {
                    var port = LinkPlus.HostRef.Port?.GetValue(resoniteLinkHost);
                    stopMethod.Invoke(resoniteLinkHost, null);
                    Plugin.Log.LogInfo($"Stopped ResoniteLink for world '{__instance.Name}' with port: {port?.ToString() ?? "unknown"}");
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Exception in World Destroy Prefix: {ex}");
        }
    }
}