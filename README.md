# LeagueBulkConvert
Convert champion models from League of Legends to glTF, with automatic textures and animations.

## How to use | [video tutorial by Luviana](https://youtu.be/CAtiX1po4Bk)
1. Extract the .zip file
2. Run LeagueBulkConvert.exe
3. Select your League of Legends installation directory
4. Select an output directory
5. Select the desired options
6. Choose which champion WADs to convert
7. Wait for the conversion to finish

## To-do
* Automatically search for a League installation directory

## Options
`League install directory`: the path to the "League of Legends" folder, e.g. "C:\Riot Games\League of Legends"

`Output directory`: the path to the directory you'd like the model files to end up, e.g. "D:\LeagueBulkConvert"

`Include skeleton`: include the skeleton for each skin

`Include animations`: include the skin's animations

`Include hidden meshes`: include meshes that would normally be hidden, like meshes used for the recall animation

`Save as separate files (.gltf)`: save the model as a .gltf, with separate .bin and .png files

`Read version 3 binary files [EXPERIMENTAL]`: temporary workaround to allow LeagueBulkConvert to work with newer League versions

## Troubleshooting
* Some files cannot be parsed because [LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit) doesn't support them. Please report these issues there!
* Some skins cannot be saved with a skeleton or with animations, either because of issues in [LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit) or [vpenades/SharpGLTF](https://github.com/vpenades/SharpGLTF) (probably the latter.) I'll look into this later.
