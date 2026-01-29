using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using BepisLocaleLoader;
using HarmonyLib;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Runtime.CompilerServices;

namespace ResoniteLinkPlus;

[BepInAutoPlugin]
[BepInDependency(BepInExResoniteShim.PluginMetadata.GUID, BepInDependency.DependencyFlags.HardDependency)]
public partial class Plugin : BasePlugin
{
    internal static new ManualLogSource Log = null!;
    internal static ConfigEntry<bool> Enabled = null!;
    internal static ConfigEntry<int> LinkStartingPort = null!;
    internal static ConfigEntry<bool> LinkAutoEnabledNewWorld = null!;

    public override void Load()
    {
        Log = base.Log;
        Enabled = Config.BindLocalized(GUID, "General", "Enabled", true, "Enables the ResoniteLinkPlus functionality, requires a restart to take effect");
        LinkStartingPort = Config.BindLocalized(GUID, "General", "LinkStartingPort", 2000, new ConfigDescription("Resonite Link port starts from this number and increments for each session", new AcceptableValueRange<int>(2000, 65535)));
        LinkAutoEnabledNewWorld = Config.BindLocalized(GUID, "General", "LinkAutoEnabledNewWorld", false, "Automatically enables ResoniteLink when creating a new world, Not recommended for public worlds");
        HarmonyInstance.PatchAll();
        Log.LogInfo($"Plugin {GUID} loaded");
    }
}

internal static class LinkPlus
{
    public class DialogControls
    {
        public Checkbox? AutoStart;
        public Checkbox? AutoPort;
        public TextField? PortField;
    }

    public static class HostRef
    {
        public static readonly Type Type = AccessTools.TypeByName("ResoniteLinkHost");
        public static readonly MethodInfo? Start = AccessTools.Method(Type, "Start", new[] { typeof(int?) });
        public static readonly PropertyInfo? Port = AccessTools.Property(Type, "Port");
        public static readonly PropertyInfo? World = AccessTools.Property(Type, "World");
    }

    public static class WorldRef
    {
        public static readonly PropertyInfo? ResoniteLink = AccessTools.Property(typeof(World), "ResoniteLink");
    }

    public static readonly ConditionalWeakTable<NewWorldDialog, DialogControls> Dialogs = new();
    public static (bool ShouldStart, int? Port) AutoStartInfo = (false, null);

    public static string GetWorldName(object resoniteLinkHostInstance)
    {
        try
        {
            var world = HostRef.World?.GetValue(resoniteLinkHostInstance) as World;
            return world?.Name ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public static int GetAvailablePort(int startingPort, int maxAttempts = 100)
    {
        for (int port = startingPort; port < startingPort + maxAttempts; port++)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                return port;
            }
            catch (SocketException)
            {
                continue;
            }
        }

        throw new Exception($"No available port found in range {startingPort}-{startingPort + maxAttempts - 1}");
    }
}