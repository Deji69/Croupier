[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/S6S2TGV0B)

- [Croupier](#croupier)
- [Features](#features)
	- [App Features](#app-features)
	- [Mod Features](#mod-features)
- [Download \& Setup](#download--setup)
	- [App](#app)
	- [Mod](#mod)
	- [Both](#both)

# Croupier

Croupier is an app and mod providing a generator of roulette spins for Hitman: World of Assassination that can be easily controlled from in-game and provides many options for customisation.

<img src="./images/CroupierV2.png" title="App interface." width="47%">
<img src="./images/CroupierV2-InGame.png" title="In-game interface." width="47%">

Go on, take it for a spin.

# Features

Croupier's features are split between an app and a mod. Use both for the most features, but both are optional. Well, you need at least one, or you're not really using Croupier, but the main features work in either...

- **Missions**: supports most, but not all main missions, bonus missions and special assignments in HITMAN™: World of Assassination.\
Excludes sniper assassin, 'Patient Zero: The Vector' and 'The Sarajevo Six' campaign, subject to change.\
The bonus missions and special assignments have not been playtested for condition difficulty, so currently ruleset difficult options do not apply, and impossible spins may be generated.

- **Mission Pool**: you choose the missions you wish to play. Presets are provided to quickly play the most common mission pool choices. Toggling missions on/off changes whether they will be spun using the 'Random' option.

- **Spin Generator**: generate roulette spins using cutting-edge pseudo-random number generation technology from the app and/or the mod.

- **Rulesets**: basic support for rulesets provided in the mod. Extended support in the app. (See: App Features -> Rulesets)

## App Features

- **Desktop Interface**: run at any time from a PC, without having to run the game or play on PC. With options for customising the presentation of the spin layout and resizing the spins. Click and drag the window to reposition it.

- **Custom Spins**: edit any spin to set up your own challenge. Select any kill method or disguise available in the mission for each target and choose complications and kill requirements.

- **Spin History**: view and revisit the last 30 spins using the History entry in the right-click menu. Choose a format for target naming using the 'Target Name Format' entry if you'd prefer an alternative to initials.

- **Copy/Paste**: spins in text form can be copied and pasted from the app to easily note down or share with others. You can also copy and paste spins from text into the app and Croupier will try its best to parse even non-standard formats.

- **Rulesets**: there are many ways to play roulette or incorporate roulette spins into gameplay, and Croupier offers options for customising the rules of spin generation to help accomodate the possibilities. For this purpose you can customise presets or select pre-defined ones to change the rules of how Croupier will generate spins.

- **Ruleset Presets**: Croupier provides ruleset presets for the current and previous Roulette Rivals tournament rulesets, as well as additional presets with alternate rules. You can also define your own custom preset to tune the rules to your liking. Supported in the mod when disconnected, but enhanced with the app. (Note: editing a ruleset in the mod will not edit any in the app - these two are treated separately.)

- **Stream-Friendly**: the app is easy to window capture in streaming/recording software and has a 'Static Mode' to aid with positioning and alignment.

## Mod Features

The mod provides in-game integration of Croupier, and can be used alone, or alongside the app to enable additional features.

- **In-Game Overlay**: for those with only one display available, a text-only in-game overlay will show the current spin conditions.

- **In-Game Controls**: no need to Alt+TAB to the app to manually respin, edit spin or revisit past spins. Stay in the game and use the ZHMModSDK UI to control the app (or the mod itself if used alone).

- **Automatic Spinning**: the mod will detect when you enter the planning screen of a mission and generate a spin for that mission automatically, unless there is already an _uncompleted_ spin active for the same mission. Spins are considered completed once you exit a mission. To disable automatic spinning, enable 'Spin Lock', which will keep the current spin active unless you manually choose to respin.

- **Mission Pool**: you can set the mission pool to choose which missions can be spun by clicking 'Random'. When connected, the mod will use and update the pool set in the app.

- **Timer**: optionally add a timer to the in-game overlay to keep track of how much time (RTA) is taken to complete a spin, including planning and restarts.

- **Spin Generator**: If the app is connected, spins will be generated from the mod, through the app. If the app is not connected, spins will be generated by a version of the generator in the mod itself.

# Download & Setup

1. There are two parts; an app, and a mod. The app provides the most features, and the mod provides integration with the game. You can use either one individually, but using them both together is recommended for the best experience.
2. The app works great standalone for generating spins, but offers no game integration features.
3. The mod is effectively a remote control for the app, though it also has a lightweight version of some features built-in.
4. See the following sections for instructions for each part of Croupier.

## App
1. Extract the contents of `App - Install Anywhere` to a location of your choosing. This does not need to be in the game installation folder.
2. Run `Croupier.exe` and enjoy.

**Linux**  
The app uses .NET and has been tested to work in Bottles using a WINE prefix.

## Mod
1. Download the latest version of [ZHMModSDK](https://github.com/OrfeasZ/ZHMModSDK/releases) and install it.
2. Extract the contents of `Mod - Install to Game` to the game installation folder (e.g. `C:\Games\HITMAN 3` - merge with existing 'Retail' folder).
3. Run the game and once in the main menu, press the `~` key (`^` on QWERTZ layouts) and enable `Croupier` from the menu at the top of the screen.
4. Use the in-game ZHMModSDK UI to control the app or generate spins using the in-game overlay.

**Linux**  
The mod has been tested to work on Linux while running the game in Proton. However, the mission planning screen detection appears to not work, seemingly due to a limitation in using the game's WinHTTP handle in this context.

## Both
* Once both are installed and running, 'Connected' should appear in the in-game Croupier UI. You can now control features of the app using the in-game UI controls, and the app will automatically spin when you enter the planning screen for a mission (unless 'Spin Lock' is enabled).
* Note you can still use the app directly to manage spins and the in-game overlay, if enabled, will show the spin from the app.