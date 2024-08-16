# MelonLoader VS Wizard

An automated Visual Studio 2022 template for creating MelonLoader mods and plugins. It supports MelonLoader 0.5.0 to the latest as well as Il2Cpp and Mono games.

## What does it handle?
It handles the creation of the required boilerplate (the `MelonMod`/`MelonPlugin` class, `MelonInfo`, and `MelonGame`) as well as referencing the required assemblies for mod development, mainly MelonLoader, Harmony, and for Il2Cpp, proxy assemblies and the unhollower (Il2CppAssemblyUnhollower or Il2CppInterop). It also handles variation between MelonLoader or Unity versions, such as framework versions or override changes.

## Usage
0. Download MelonLoader to your game and run it once before continuing.
1. Download the VSIX from the [Releases](https://github.com/TrevTV/MelonLoader.VSWizard/releases) tab.
2. Close all instances of Visual Studio and run the VSIX installer (double-clicking it should open it).
3. Open Visual Studio and create a new project.
4. Search for `MelonLoader` and click on either Mod or Plugin.
5. Enter the project info and press Create.
6. Select the EXE of the game you are modding and press Open.
7. Wait for the project creation and it should open a Visual Studio window with a working project.

You may want to change the author in the `MelonInfo` attribute. It defaults to your computer's username.

## Licensing
- [AssetRipper.Primitives](https://github.com/AssetRipper/Primitives) is licensed under the MIT License. See [LICENSE](https://github.com/AssetRipper/Primitives/blob/master/License.md) for the full License.
- [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET) is licensed under the MIT License. See [LICENSE](https://github.com/nesrak1/AssetsTools.NET/blob/master/LICENSE) for the full License.
- [StrongNamer](https://github.com/dsplaisted/strongnamer) is licensed under the MIT License. See [LICENSE](https://github.com/dsplaisted/strongnamer/blob/master/LICENSE) for the full License.
- [MelonLoader](https://github.com/LavaGang/MelonLoader) is licensed under the Apache License, Version 2.0. See [LICENSE](https://github.com/LavaGang/MelonLoader/blob/master/LICENSE.md) for the full License.
