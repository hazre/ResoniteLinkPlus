# ResoniteLinkPlus
[![Thunderstore Badge](https://modding.resonite.net/assets/available-on-thunderstore.svg)](https://thunderstore.io/c/resonite/)

A [Resonite](https://resonite.com/) mod that enhances ResoniteLink functionality.

> [!NOTE]
> Some of these features will likely eventually be integrated into the vanilla game (I assume). This mod will be updated and features will be marked as obsolete or removed as that happens.

## Features
- [x] Enable ResoniteLink and choose port in the New World dialog
- [x] Auto-increment ports from a chosen starting port
- [x] Cleanup ResoniteLink when world closes
- [x] Option to auto enable ResoniteLink for all new worlds
- [ ] Stop Button ResoniteLink in the Session Tab
- [ ] Change port in the Session Tab
- [ ] gRPC support (no promises, I need to look into it but it's on the wishlist)

## Installation (Manual)
1. Install [BepisLoader](https://github.com/ResoniteModding/BepisLoader) for Resonite.
2. Download the latest release ZIP file (e.g., `hazre-ResoniteLinkPlus-1.0.0.zip`) from the [Releases](https://github.com/hazre/ResoniteLinkPlus/releases) page.
3. Extract the ZIP and copy the `plugins` folder to your BepInEx folder in your Resonite installation directory:
   - **Default location:** `C:\Program Files (x86)\Steam\steamapps\common\Resonite\BepInEx\`
4. Start the game. If you want to verify that the mod is working you can check your BepInEx logs.

## Configuration

The mod can be configured via `BepInEx/config/dev.hazre.resonitelinkplus.cfg`:

- `Enabled`: (Default: `true`) Enables the mod.
- `LinkStartingPort`: (Default: `2000`) Starting port for automatic discovery.
- `LinkAutoEnabledNewWorld`: (Default: `false`) Automatically enables ResoniteLink when creating a new session.

## Acknowledgements

- [ResonitePluginSelect](https://github.com/Nytra/ResonitePluginSelect) served as reference to create the NewWorldDialog patch.

## License

This project is licensed under MIT License. See [LICENSE](LICENSE) for details.