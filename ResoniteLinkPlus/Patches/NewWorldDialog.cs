using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace ResoniteLinkPlus;

[HarmonyPatch(typeof(NewWorldDialog), "BuildUI")]
public class NewWorldDialogBuildUIPatch
{
    static void Postfix(NewWorldDialog __instance)
    {
        if (!Plugin.Enabled.Value) return;

        __instance.RunInUpdates(3, () =>
        {
            if (__instance.Slot.FilterWorldElement() is null) return;

            try
            {
                var mainUi = new UIBuilder(__instance.Slot);
                RadiantUI_Constants.SetupDefaultStyle(mainUi);
                mainUi.VerticalLayout(4f);
                mainUi.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);
                mainUi.Style.MinHeight = 32f;
                mainUi.Style.PreferredHeight = 32f;

                mainUi.NestInto(__instance.Slot[2][1][0]);

                var autoStartCheckbox = mainUi.Checkbox("World.Config.ResoniteLinkHeader".AsLocaleKey(), state: Plugin.LinkAutoEnabledNewWorld.Value);

                mainUi.HorizontalLayout(4f);
                mainUi.Text("NewWorld.Port".AsLocaleKey());

                var autoLinkPort = mainUi.Checkbox("NewWorld.AutoPort".AsLocaleKey(), state: true);
                var linkPortField = mainUi.TextField();

                // Set initial port to next available
                int nextPort = LinkPlus.GetAvailablePort(Plugin.LinkStartingPort.Value);
                linkPortField.TargetString = nextPort.ToString();

                mainUi.NestOut();

                // Store references
                LinkPlus.Dialogs.GetOrCreateValue(__instance).AutoStart = autoStartCheckbox;
                LinkPlus.Dialogs.GetOrCreateValue(__instance).AutoPort = autoLinkPort;
                LinkPlus.Dialogs.GetOrCreateValue(__instance).PortField = linkPortField;

                Plugin.Log.LogDebug("ResoniteLinkPlus UI added to NewWorldDialog");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Exception in BuildUI Postfix: {ex}");
            }
        });
    }
}

[HarmonyPatch(typeof(NewWorldDialog), "OnCommonUpdate")]
public class NewWorldDialogUpdatePatch
{
    static void Postfix(NewWorldDialog __instance)
    {
        if (!Plugin.Enabled.Value) return;

        try
        {
            if (LinkPlus.Dialogs.TryGetValue(__instance, out var controls) &&
                controls.AutoPort != null &&
                controls.PortField != null)
            {
                // Enable/disable port field based on auto port checkbox
                var button = controls.PortField.Slot.GetComponent<Button>();
                if (button != null)
                {
                    button.Enabled = !controls.AutoPort.IsChecked;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogDebug($"Exception in OnCommonUpdate Postfix: {ex}");
        }
    }
}

[HarmonyPatch(typeof(NewWorldDialog), "OnStartSession")]
public class NewWorldDialogStartSessionPatch
{
    static void Prefix(NewWorldDialog __instance)
    {
        if (!Plugin.Enabled.Value) return;

        try
        {
            if (!LinkPlus.Dialogs.TryGetValue(__instance, out var controls) ||
                controls.AutoStart == null || !controls.AutoStart.IsChecked)
            {
                Plugin.Log.LogDebug("Auto-start ResoniteLink is disabled or UI not found, skipping");
                return;
            }

            if (controls.AutoPort == null || controls.PortField == null)
            {
                Plugin.Log.LogWarning("Port controls not found, cannot auto-start ResoniteLink");
                return;
            }

            int? linkPort = null;

            if (!controls.AutoPort.IsChecked)
            {
                // User specified a port
                if (int.TryParse(controls.PortField.TargetString, out int specifiedPort))
                {
                    linkPort = specifiedPort;
                }
                else
                {
                    Plugin.Log.LogWarning("Invalid ResoniteLink port specified, using auto port");
                }
            }

            LinkPlus.AutoStartInfo = (true, linkPort);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Exception in OnStartSession Prefix: {ex}");
        }
    }
}