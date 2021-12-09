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


## Install

1. Copy EmulatedMediaGuide.exe and related DLLs to the server.
2. Ensure that "NT AUTHORITY\LocalService" has read and execute access to EmulatedMediaGuide.exe, related DLLs, and the directory.

Example of changing ACLs in PowerShell.

```powershell
$acl = Get-Acl .\EmulatedMediaGuide.exe
$AccessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\LocalService","ReadAndExecute","Allow")
$acl.SetAccessRule($AccessRule)
$acl | Set-Acl .\EmulatedMediaGuide.exe
$acl | Set-Acl .\Newtonsoft.Json.dll
$acl | Set-Acl .\uhttpsharp.dll
```

3. Using an admin console, register the EmulatedMediaGuide service.
```cmd
EmulatedMediaGuide.exe --install
```
You can remove the service with the `--uninstall` argument.

4. Configure the M3U URL, EPG URL and Filter in the config file.

Settings for EmulatedMediaGuide are stored in the same directory as the exectuable, called config.ini. 

| Category | Setting | Value | Description | Default Value |
| --- | --- | --- | --- | --- |
| Server | IpAddress | string | IP address on which to listen. | 127.0.0.1 |
| Server | Port | int | Port number on which to listen. | 6079 |
| Server | StartChannel | int | Starting channel number for IPTV lineup. | 1 |
| Data | M3UURL | string | URL of the M3U from your provider. | **Required** |
| Data | EPGURL | string | URL of the EPG from your provider. | **Required** |
| Data | Filter | string | Regular expression to include channels. | **Required** |

You may also configure additional settings like bind IP address, port, starting channel number, etc.

```
[Server]
IpAddress=127.0.0.1
Port=6079
StartChannel=1
[Data]
m3uUrl=http://yourwebsite.com/emuguide/yourcustomlineup.m3u
epgUrl=https://yourwebsite.com/emuguide/yourcustomlineup.xml
[Regex]
Filter=.*
```

5. Start the service with `net start EmulatedMediaGuide`. If everything is configured correctly, the Application event log should report that EmulatedMediaGuide has begun updating the lineup and EPG. If you get "access denied," ensure you have completed step 2.

### Update EPG

Create a scheduled task using Windows' Task Scheduler to periodically update the EPG. The scheduled task should invoke `EmulatedMediaGuide.exe --update-epg`. This will regenerate the local epg.xml so that Plex can update its guide. I recommend scheduling this to run 15 minutes before the "Scheduled Tasks" window (2am - 5am by default) in Plex.

#### Missing Channel Logos

EmulatedMediaGuide uses the logo URL in your provider's XML for each channel. If no URL is provided, EmulatedMediaGuide will generate a basic logo image using the channel name. Some Plex clients (e.g.: Roku) do not do this automatically. This feature is intended to make it easer to locate a channel when seeking in the grid guide.

The following setting entries configure the color and font for generated channel logo images:

| Category | Setting | Value | Description | Default Value |
| --- | --- | --- | --- | --- |
| Style | LogoFontFamily | string | Font family used in logo. | Segoe UI |
| Style | LogoColor | string | Hex color value for text in logo. | 0xFFDCDCDC |
| Style | LogoBackground | string | Hex color value for background in logo. | 0x1 |

```
[Style]
LogoFontFamily=Segoe UI
LogoColor=#DCDCDC
LogoBackground=0x1
```

Note: The magic value 0x1 in LogoBackground means "select a dynamic background color using the channel name."

Note: EmulatedMediaGuide does not check if the logo URLs resolve. If you are missing a logo for a channel, the URL in your provider's EPG data may be broken.

#### Program Guide Gap Fill

Some Plex clients (e.g.: Plex, Android) fail to display channels in the grid view if the channel has no guide data. The web interface displays these channels using an "Unknown Airing" placeholder. As a workaround for those clients, EmulatedMediaGuide can insert dummy episodes on half-hour intervals to ensure all channels appear on buggy clients. To enable this feature, set GapFillAmount to a value greater than zero.

The following setting entries configure the gap filling behaviour:

| Category | Setting | Type | Description | Default Value |
| --- | --- | --- | --- | --- |
| Style | GapFillAmount | int | Number of hours to fill with dummy episodes starting from midnight today. | 0 |
| Style | GapFillTitle | string | Title for dummy episodes that appear in the guide | Unknown Airing |

```
[Style]
GapFillAmount=0
GapFillTitle=Unknown Airing
```

#### Program Logging

The program logs to the Event Log regardless of this setting, however it can also be configured to write to a log file on disk. This is enabled by default, and the log directory is set to "{Application Directory}\logs\".

The following setting entries configure the logging behaviour:

| Category | Setting | Type | Description | Default Value |
| --- | --- | --- | --- | --- |
| Logging | LogFolder | string | Folder to write logs into | Application Directory |
| Logging | Enabled | bool | Should logging be enabled | true |

```
[Logging]
LogFolder=C:\Jellyfin.EmuGuide\logs
Enabled=true
```

## Debug/Develop

1. Compile the project and register the executable from the bin/Debug folder as per the install guide.
2. In an admin console, use `net start EmulatedMediaGuide` to start the service.
3. In Visual Studio, attach to EmulatedMediaGuide.exe using __Debug > Attach to Process...__

When compiled in DEBUG mode, the service should wait in Service.cs#Onstart for a debugger to attach before proceeding.