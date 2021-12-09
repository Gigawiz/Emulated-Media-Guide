# Emulated Media Guide

The invention of Plex and JellyFin to host your own Netflix-esq service was a great idea. However it lacks a tv "guide" experience. What if you don't know what you want to watch? There are also those of us running media servers to help our less technically inclined parents get off of cable. This is intended to be that "Guide" experience.

The program itself is a service (originally based off the source code of MarkLieberman's [IPTvTuner](https://github.com/marklieberman/iptvtuner)), that runs in the background providing an IPTV interface for Plex, Emby, JellyFin, etc to connect to as if it were a HDHomeRun tuner (or equivalent).

This service can be configured to create your own "channels" from the media existing on your device, then serves it to your media server as if the channel lineup came from a service provider.

Currently, the service itself is a WIP. My main focus is to get the service running, and accepting a custom content feed. After that, I'll be focusing on plugins for specific media servers (like plex, emby, etc).

Keep an eye to this repo. As I make changes, I'll also be making releases that you can use as a "preview" (please, let me know of any bugs you find so I can iron out the kinks).

Otherwise, enjoy the project, I hope it helps fulfill your media server "guide" needs!

-Gigawiz


## Features
- [x] Runs as a Windows Service in the background
- [x] Allows custom "channel" creation
- [x] Emulates an IPTV Tuner (such as the HDHomeRun)
- [x] Configurable IP/Port binding
- [x] Custom Styling (Including Font and logo settings)
- [x] Logs both to Event Log and a custom log file (configurable location)
- [x] Can retrieve data from external or internal sources

## Planned Features
- [ ] Mimic official cable tv lineups based on what media you have on your device
- [ ] Custom tool to create tv lineups (instead of current text-based creation)
- [ ] Custom plugins to interface with JellyFin, Emby, Plex, (more?)
- [ ] Custom Branding?
- [ ] ***Maybe*** Ability to pull from streaming websites (like you can do with a firestick) ***Maybe***


### The Configuration File
```
[Server]
IpAddress=127.0.0.1
Port=6079
StartChannel=1
[Style]
LogoFontFamily=Segoe UI
LogoColor=#DCDCDC
LogoBackground=1
GapFillAmount=0
GapFillTitle=Unknown Airing
[Logging]
LogFolder=C:\Jellyfin.EmuGuide\logs
Enabled=true
[Data]
m3uUrl=http://yourwebsite.com/emuguide/yourcustomlineup.m3u
epgUrl=https://yourwebsite.com/emuguide/yourcustomlineup.xml
[Regex]
Filter=.*
```

