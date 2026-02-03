using FrooxEngine;
using MonoDetour;
using MonoDetour.HookGen;
using Elements.Core;
using FrooxEngine.Store;

namespace ResoniteLinkPlus;

[MonoDetourTargets(typeof(World))]
internal static class WorldPatches
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Md.FrooxEngine.World.StartSession.Postfix(Postfix_StartSession);
        Md.FrooxEngine.World.Destroy.Prefix(Prefix_Destroy);
    }

    static void Postfix_StartSession(ref WorldManager manager, ref WorldAction init, ref ushort port, ref string forceSessionId, ref DataTreeNode load, ref Record record, ref bool unsafeMode, ref IEnumerable<AssemblyTypeRegistry> assemblies, ref World returnValue)
    {
        if (!Plugin.Enabled.Value || returnValue == null) return;

        var (shouldStart, requestedPort) = LinkPlus.AutoStartInfo;
        LinkPlus.AutoStartInfo = (false, null);

        if (!shouldStart) return;

        var world = returnValue;
        world.RunInUpdates(3, () =>
        {
            if (world.IsDestroyed) return;
            try
            {
                world.StartResoniteLink(requestedPort);
                var port = world.ResoniteLink?.Port;
                Plugin.Log.LogInfo($"World '{world.Name}': Started ResoniteLink on port {port?.ToString() ?? "auto"}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"World '{world.Name}': Failed to start ResoniteLink - {ex}");
            }
        });
    }
    static void Prefix_Destroy(World self)
    {
        if (!Plugin.Enabled.Value || self == null) return;

        if (self.ResoniteLink == null)
        {
            Plugin.Log.LogError($"World '{self.Name}': ResoniteLink property not found");
            return;
        }

        var resoniteLinkHost = self.ResoniteLink;
        var port = resoniteLinkHost.Port;
        resoniteLinkHost.Dispose();
        Plugin.Log.LogInfo($"World '{self.Name}': Stopped ResoniteLink on port {port}");
    }
}