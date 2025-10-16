# MelonLoader FSR-like Upscaler

This repository contains a MelonLoader mod that emulates a simplified AMD FSR style workflow by
rendering Unity games at a lower internal resolution while keeping the display resolution unchanged.
The mod uses Unity's dynamic resolution APIs (`QualitySettings.resolutionScalingFixedDPIFactor` and
`ScalableBufferManager`) and enables dynamic resolution support on all cameras that expose it.

> **Note**
> The mod does not ship with AMD's proprietary algorithms. It relies entirely on Unity's public
> dynamic resolution pipeline. Depending on the game engine version, visual results can vary from
> native FSR.

## Features

- Configurable internal render scale with sensible bounds (50% – 100%).
- Toggle the entire feature on/off without uninstalling the mod.
- Optional "refresh every frame" mode for games that reset dynamic resolution.
- Automatically enables `allowDynamicResolution` on cameras when scenes load.

## Building the mod

1. Install the [.NET SDK 6.0 or later](https://dotnet.microsoft.com/en-us/download).
2. Copy the required game assemblies into the `lib` folder so the project can reference them:
   ```
   lib/
     MelonLoader/MelonLoader.dll
     UnityEngine/UnityEngine.CoreModule.dll
     UnityEngine/UnityEngine.IMGUIModule.dll
   ```
   The exact set of UnityEngine modules you need depends on the game. At minimum, the two listed
   above are required for the stock script.
3. Build the project:
   ```bash
   dotnet build src/MelonLoaderFSR/MelonLoaderFSR.csproj -c Release
   ```
4. Copy the generated `MelonLoaderFSR.dll` from `src/MelonLoaderFSR/bin/Release/net472/` into the
   game's `Mods` folder (next to the other MelonLoader mods).

## Configuration

Use MelonLoader's built-in Preferences UI or edit the generated configuration file at
`UserData/ModPrefs.cfg` to tweak the following entries under the `FSRLikeUpscaler` category:

| Setting              | Description                                                                                          |
|----------------------|------------------------------------------------------------------------------------------------------|
| `Enabled`            | Master toggle for the downscaler.                                                                    |
| `RenderScale`        | Internal resolution multiplier. Lower values mean more downscaling (0.5 = 50% of the display res).   |
| `RefreshEveryFrame`  | Forces the mod to reapply the render scale each frame. Enable if the game keeps restoring native res. |

## Limitations

- The game must use Unity's dynamic resolution system. Some older titles ignore
  `ScalableBufferManager` calls, which means the mod cannot affect them.
- The mod does not include a sharpening pass. You can combine it with other post-processing mods if
  you need image sharpening.
- Because the project references proprietary game assemblies, the repository cannot bundle them.
  Place them in the `lib` folder as described above before building.

## License

This project is released under the MIT license. See [LICENSE](LICENSE) for details.
