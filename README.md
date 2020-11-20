# LeagueBulkConvert
Convert champion models from League of Legends to glTF, with automatic textures and animations.

## How to use | [video tutorial by Luviana](https://youtu.be/CAtiX1po4Bk)
1. Extract the .zip file
2. Run LeagueBulkConvert.exe
3. Select your League of Legends installation directory
4. Select an output directory
5. Optionally edit the config file
6. Click convert

## To-do
* Add a GUI for editing the configuration
* Allow the conversion to be cancelled
* Automatically search for a League installation directory

## Options
`League install directory`: the path to the "League of Legends" folder, e.g. "C:\Riot Games\League of Legends"

`Output directory`: the path to the directory you'd like the .glb files to end up, e.g. "D:\LeagueBulkConvert" (make sure this directory already exists)

`Include skeletons`: include the skeleton for each skin

`Include animations`: include the skin's animations, which requires the skeleton

`Include hidden meshes`: include meshes that would normally be hidden, like meshes used for the recall animation and also includes any meshes that are in "IgnoreMeshes" in the config file

`Save binary files and textures separately`: save the model as a .gltf, with separate .bin and .png files

## config.json
`IncludeOnly`: a list of wad.client filenames that should be extracted, extracts everything by default (the smaller this list the faster the conversion, e.g. `includeOnly: ["Aatrox.wad.client"]`)

`IgnoreMeshes`: here you can specify individual meshes to be ignored for specific skins (an example is given in the default config, enabling "Include hidden meshes" overrides this)

`IgnoreCharacters`: a list of characters that are ignored (you probably don't need to change this)

`ScaleList`: a list of scales I've calculated from the champion's official confirmed heights to resize the model to real life size (you probably don't need to change this)

`ExtractFormats`: files with these extensions will be extracted (you probably don't need to change this)

`SamplerNames`: samplers with these names will be read to see if there is a texture (you probably don't need to change this)

## Troubleshooting
* You need to be connected to the internet to use this program, since it downloads textfiles from GitHub (will be fixed soon)
* You should select an empty directory for the converted files, since the program automatically creates and deletes entire directories recursively
* Some files cannot be parsed because [LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit) doesn't support them. Please report these issues there!
* Some skins cannot be saved with a skeleton or with animations, either because of issues in [LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit) or [vpenades/SharpGLTF](https://github.com/vpenades/SharpGLTF) (probably the latter.) I'll look into this later.
* The config.json file is case-sensitive!
