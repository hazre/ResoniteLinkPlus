using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using MonoDetour;
using MonoDetour.HookGen;

namespace ResoniteLinkPlus;

[MonoDetourTargets(typeof(NewWorldDialog))]
internal static class NewWorldDialogPatches
{
    [MonoDetourHookInitialize]
    static void Init()
    {
        Md.FrooxEngine.NewWorldDialog.BuildUI.Postfix(Postfix_BuildUI);
        Md.FrooxEngine.NewWorldDialog.OnCommonUpdate.Postfix(Postfix_OnCommonUpdate);
        Md.FrooxEngine.NewWorldDialog.OnStartSession.Prefix(Prefix_OnStartSession);
    }
    static void Postfix_BuildUI(NewWorldDialog self, ref Action<NewWorldDialog> customStart, ref Action<UIBuilder> customPresetUI)
    {
        if (!Plugin.Enabled.Value) return;

        self.RunInUpdates(3, () =>
        {
            if (self.Slot.FilterWorldElement() is null) return;

            var mainUi = new UIBuilder(self.Slot);
            RadiantUI_Constants.SetupDefaultStyle(mainUi);
            mainUi.VerticalLayout(4f);
            mainUi.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);
            mainUi.Style.MinHeight = 32f;
            mainUi.Style.PreferredHeight = 32f;

            mainUi.NestInto(self.Slot[2][1][0]);

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
            LinkPlus.Dialogs.GetOrCreateValue(self).AutoStart = autoStartCheckbox;
            LinkPlus.Dialogs.GetOrCreateValue(self).AutoPort = autoLinkPort;
            LinkPlus.Dialogs.GetOrCreateValue(self).PortField = linkPortField;

            Plugin.Log.LogDebug("NewWorldDialog: Added ResoniteLinkPlus UI controls");
        });
    }
    static void Postfix_OnCommonUpdate(NewWorldDialog self)
    {
        if (!Plugin.Enabled.Value) return;

        if (LinkPlus.Dialogs.TryGetValue(self, out var controls) &&
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
    static void Prefix_OnStartSession(NewWorldDialog self, ref IButton button, ref ButtonEventData eventData)
    {
        if (!Plugin.Enabled.Value) return;

        if (!LinkPlus.Dialogs.TryGetValue(self, out var controls) ||
            controls.AutoStart == null || !controls.AutoStart.IsChecked)
        {
            Plugin.Log.LogDebug("NewWorldDialog: ResoniteLink auto-start disabled, skipping");
            return;
        }

        if (controls.AutoPort == null || controls.PortField == null)
        {
            Plugin.Log.LogWarning("NewWorldDialog: Port controls not found, cannot auto-start ResoniteLink");
            return;
        }

        int? linkPort = null;

        if (!controls.AutoPort.IsChecked)
        {
            if (int.TryParse(controls.PortField.TargetString, out int specifiedPort))
            {
                linkPort = specifiedPort;
            }
            else
            {
                Plugin.Log.LogWarning("NewWorldDialog: Invalid port specified, using auto-assignment");
            }
        }

        LinkPlus.AutoStartInfo = (true, linkPort);
    }
}