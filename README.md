# LeagueBulkConvert
Easily convert champion models from League of Legends to glTF.

## How do I use it?
1. Extract the .zip file
2. Run LeagueBulkConvert.exe
3. Select your League of Legends installation directory
4. Select an output directory
5. Optionally edit the config file
6. Click convert

## Options
`League install directory`: the path to the "League of Legends" folder, e.g. "C:\Riot Games\League of Legends"

`Output directory`: the path to the directory you'd like the .glb files to end up, e.g. "D:\LeagueBulkConvert" (make sure this directory already exists)

`Include skeletons`: include the skeleton for each skin

`Include animations`: include the skin's animations, which requires the skeleton

`Include hidden meshes`: include meshes that would normally be hidden, like meshes used for the recall animation

`Show error pop-ups`: enable the various error pop-ups

`Thread count`: how many threads to use for conversion (limited to whichever is smaller: the amount of threads your processor has, or the amount of installed RAM in GiB divided by two)

## config.json
`extractFormats`: files with these extensions will be extracted (you probably don't need to change this)

`ignoreCharacters`: a list of characters that are ignored (you probably don't need to change this)

`ignoreMeshes`: here you can specify individual meshes to be ignored for specific skins (an example is given in the default config)

`includeOnly`: a list of wad.client filenames that should be extracted (you want this to be as small as possible, e.g. `includeOnly: ["Aatrox.wad.client"]`)

`samplerNames`: samplers with these names will be read to see if there is a texture (you probably don't need to change this)

`scaleList`: a list of scales I've calculated from the champion's official confirmed heights to resize the model to real life size (you probably don't need to change this)

## Troubleshooting
Since very few people have used this program so far and since I've not experienced any issues during my testing, I don't really have anything to put here.

* You need to be connected to the internet to use this program, since it downloads textfiles from GitHub
* You should select an empty directory for the converted files, since the program automatically creates and deletes entire directories recursively.
