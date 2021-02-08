# LeagueBulkConvert

Convert champion models from League of Legends to glTF, with automatic textures
and animations.

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
* Add support for
  [LoL-Fantome/LeagueDownloader](https://github.com/LoL-Fantome/LeagueDownloader)

## Options

* **League install directory**: the path to the "League of Legends" folder, e.g.
  "C:\Riot Games\League of Legends"

* **Output directory**: the path to the directory you'd like the model files to
  end up, e.g. "D:\LeagueBulkConvert"

* **Include skeleton**: include the skeleton for each skin

* **Include animations**: include the skin's animations

* **Include hidden meshes**: include meshes that would normally be hidden, like
  meshes used for the recall animation

* **Save as separate files (.gltf)**: save the model as a .gltf, with separate
  .bin and .png files

* **Read version 3 binary files \[EXPERIMENTAL\]**: temporary workaround to
  allow LeagueBulkConvert to work with newer League versions

## Troubleshooting

### Why can't some files be parsed?

Some files can't be parsed, because
[LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit)
doesn't support them. Please open an issue there!

### Why can't some skins be saved?

Some skins can't be saved with a skeleton or with animations, probably because
of issues in [vpenades/SharpGLTF](https://github.com/vpenades/SharpGLTF). I
might look into this later.

### What happened to the `IgnoreMeshes` option?

I got rid of it because I couldn't think of any good use cases. If you need this
option, feel free to open an issue.

### What happened to the `IgnoreCharacters` option?

I initially added this option to cut down on unnecessary export directories. Now
that you have to manually select the WADs you want to convert, this option
doesn't make sense anymore. I won't be adding this option back in.

### What happened to the `ScaleList` option?

I got rid of it because the default setting is pretty much as good as it gets.
If you need this option, feel free to open an issue.

### What happened to the `ExtractFormats` option?

I got rid of it because this option didn't make sense in the first place. This
program should only extract files that it needs for conversion. If you want to
extract other files from WADs, use
[Crauzer/Obsidian](https://github.com/Crauzer/Obsidian).

### What happened to the `SamplerNames` option?

I got rid of it because the vast majority of users will never need to change
this option and because it isn't up to the user to add sampler names. If any
sampler names are missing, please open an issue.
