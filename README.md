# LeagueConvert

Effortlessly convert League of Legends models to glTF, with textures and
animations.

## Installation instructions

1. Download and install the
   [.NET 7.0 Runtime](https://dotnet.microsoft.com/download/dotnet/7.0/runtime). If you installed an older .NET Runtime for this program, you can uninstall it.
2. Download and extract the latest build of LeagueConvert for your platform 
   [here](https://github.com/Jochem-W/LeagueConvert/actions).
   1. Alternatively, you can navigate to the "Actions" tab at the top, which should lead you to the same place. Note that build artifacts might expire.
   2. Generic is a cross-platform build, but it might not run on platforms that aren't explicitly listed due to missing libraries. I might not be able to provide support for unsupported platforms.
3. Run the executable, e.g. `LeagueConvert.CommandLine.exe`, or `dotnet LeagueConvert.CommandLine.dll`

## Status and roadmap

This version was built from scratch to replace the GUI version
(LeagueBulkConvert) and is still in development. Currently this version is
command-line only. The builds from GitHub Actions probably work just fine, but
there are still a few things that have to be done before a release is created.
I highly recommend not using the release builds anymore because they have many
issues that have all been fixed in this version.

### Comparison to old version

|                                        |   Old |                New |
|:---------------------------------------|------:|-------------------:|
| Aatrox conversion time                 |  16 s |               10 s |
| Aatrox conversion time (skeleton)      |  16 s |               10 s |
| Aatrox conversion time (animations)    | 122 s |               24 s |
| (More) correct animations              |   :x: | :heavy_check_mark: |
| Cross platform                         |   :x: | :heavy_check_mark: |
| No temporary files                     |   :x: | :heavy_check_mark: |
| Smart update check                     |   :x: | :heavy_check_mark: |
| Support for newer models with tangents |   :x: | :heavy_check_mark: |
| Valid glTF                             |   :x: | :heavy_check_mark: |

### SimpleGltf

SimpleGltf is a custom library that was written to replace SharpGltf. This
library probably won't be released as a standalone library.

Planned:

* Optimisation/cleanup
* Reading glTF

### LeagueToolkit

This is a modified version of
[LoL-Fantome/LeagueToolkit](https://github.com/LoL-Fantome/LeagueToolkit/). and
will probably be moved to a separate repository in the future.

### LeagueConvert

LeagueConvert is a custom library that searches for game models, textures and
animations.

Planned:

* Rewrite hash table loading code
* Support for map geometry
* Support for textures other than diffuse
* Move code from LeagueConvert.CommandLine to LeagueConvert
* Optimisation/clean-up

### LeagueConvert.CommandLine

LeagueConvert.CommandLine is the command-line interface for LeagueConvert.

Planned:

* Clean-up

### LeagueConvert.MAUI

A multiplatform user interface for LeagueConvert. Postponed for now.

## Set-up instructions for developing

1. Clone or download the source code.
2. Install the .NET 7.0 SDK. (Recommended: Visual Studio 2022 with the '.NET
   desktop development' workload and the 'Development tools for .NET'
   component.)
   1. Optionally Install Rider.
3. Open the .sln.
