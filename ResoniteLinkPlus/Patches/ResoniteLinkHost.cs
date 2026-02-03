using FrooxEngine;
using MonoDetour;
using MonoDetour.HookGen;

namespace ResoniteLinkPlus;

[MonoDetourTargets(typeof(ResoniteLinkHost))]
internal static class ResoniteLinkHostStartPatch
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Md.FrooxEngine.ResoniteLinkHost.Start.Prefix(Prefix_Start);
        Md.FrooxEngine.ResoniteLinkHost.Start.Postfix(Postfix_Start);
    }
    static void Prefix_Start(ResoniteLinkHost self, ref int? port)
    {
        if (!Plugin.Enabled.Value) return;

        if (port == null)
        {
            int startingPort = Plugin.LinkStartingPort.Value;
            int availablePort = LinkPlus.GetAvailablePort(startingPort);
            port = availablePort;

            Plugin.Log.LogInfo($"World '{self.World.Name}': No port specified, assigned port {availablePort}");
        }

        return;
    }

    static void Postfix_Start(ResoniteLinkHost self, ref int? port, ref bool returnValue)
    {
        if (returnValue)
        {
            if (self?.Port != null)
            {
                Plugin.Log.LogInfo($"World '{self.World.Name}': ResoniteLink port set to {self.Port}");
            }
        }
        else
        {
            Plugin.Log.LogWarning($"World '{self.World.Name}': Failed to set ResoniteLink port to {port}");
        }
    }
}