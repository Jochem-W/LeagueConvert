# LeagueConvert

Easily convert champion models from League of Legends to glTF, complete with
textures and animations.

## What is 'vnext'

On this branch I'll be working towards the next version of LeagueConvert
(formerly LeagueBulkConvert, might still be changed). There is a still a
lot I have to do and I hope to release this version when .NET MAUI is
generally available.

### Biggest changes compared to LeagueBulkConvert

* No more temporary 'assets' directories
* Much more granular control over skin parsing, loading and saving
* Cross-platform command-line version (theoretically, haven't tested this yet)

## How to use

1. Download the latest source code .zip archive
2. Install the .NET 5.0 SDK (optionally you can use an IDE like Rider or
Visual Studio)
3. Execute `dotnet run` in the 'LeagueConvert/LeagueConvert.CommandLine' folder

## To-do

* Extend command line functionality
* Add GUI (cross-platform)
* Feature-parity with the existing LeagueBulkConvert
