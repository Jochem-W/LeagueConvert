# LeagueConvert

Easily convert champion models from League of Legends to glTF, complete with
textures and animations.

## What is 'vnext'

On this branch I'll be working towards the next version of LeagueConvert
(formerly LeagueBulkConvert, might still be changed).

## Status and roadmap

Initially I wanted to rewrite all of LeagueBulkConvert into a library, a command
line version and a GUI version. I also wanted to add support for map geometry
files, but I ran into a couple of issues. I decided the best way forward would
be to create my own glTF writing library and also modify LeagueToolkit.

### SimpleGltf

Currently, all necessary features have been implemented and files created by
SimpleGltf pass glTF validation.

The following table contains the conversion times when using SharpGLTF (old) and
SimpleGltf (new). SimpleGltf yields an increase in speed of up to 5 times!

| Mode            | Old time | New time |
|:--------------- | --------:| --------:|
| MeshAndTextures | 16,5 s   | 10,4 s   |
| WithSkeleton    | 17,8 s   | 10,5 s   |
| WithAnimations  | 136,2 s  | 27,6 s   |

To-do:

* Optimisation/cleanup

### LeagueConvert

Currently, LeagueConvert can do everything LeagueBulkConvert can do, but with
code that isn't super terrible.

Planned:

* Rewrite hash table loading code
* Support for map geometry
* Support for texture masks (if possible)
* Optimisation/cleanup
* Fix inconsistencies in texture loading

### LeagueConvert.CommandLine

Postponed until I am done with LeagueConvert. Most of the code for a
command-line version is already in place.

### LeagueConvert.MAUI

A multiplatform user interface for LeagueConvert. Postponed until all other
work has been finished.

## Biggest changes compared to LeagueBulkConvert

* No more temporary 'assets' directories
* Much more granular control over skin parsing, loading and saving
* Cross-platform (theoretically and without macOS support)
* Up to 5 times faster (see [SimpleGltf](#simplegltf))

## Installation instructions

1. Download the
   [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0/runtime)
3. Download the latest build (generic is cross-platform)
   [here](https://github.com/Jochem-W/LeagueBulkConvert/actions?query=branch%3Avnext).
3. Extract the archive
4. Run the executable or `dotnet LeagueConvert.CommandLine.dll`

## Set-up instructions for developing

1. Clone the source code
2. Install the .NET 6.0 SDK. I recommend installing Visual Studio 2022 with the
   '.NET desktop development' workload and the optional 'Development tools for
   .NET' component
3. Optionally install Rider, which is in my opinion a much better IDE
4. Open the .sln
