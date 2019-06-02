## MyPubgTelemetry - A PUBG Telemetry Downloader and Stats Analyzer

[![GitHub license](https://img.shields.io/github/license/MikeClark512/MyPubgTelemetry.svg?color=brightgreen)](https://github.com/MikeClark512/MyPubgTelemetry/blob/master/LICENSE) [![GitHub issues](https://img.shields.io/github/issues/MikeClark512/MyPubgTelemetry.svg)](https://github.com/MikeClark512/MyPubgTelemetry/issues)

![main](https://github.com/MikeClark512/MyPubgTelemetry/blob/master/site/screenshots/main.png)

### Features
1. Downloads, saves, and caches all available telemetry and match metadata for one or more players
1. Previously downloaded telemetry that is no longer available online is preserved, so you can accumulate historical data
1. View match stats as a squad total, or for individual players
1. Chart of hitpoints over time for each match for each squadmate, calculated from telemetry (zoomable)
1. Export data table to CSV for analysis in Excel or whatever tool you want
1. Search for downloaded matches by date, playername, or ID
1. Right-click on any individual match to view the match on pubglookup.com
1. Right-click on any individual match to reveal its telemetry data file in Explorer, in case you like reading JSON
1. Async/on-demand data loading is able to handle as many gigabytes of telemetry as you want to save and load

### Download and Use
1. Download [MyPubgTelemetry-v0.0.3.zip](https://github.com/MikeClark512/MyPubgTelemetry/releases/download/v0.0.2/MyPubgTelemetry-v0.0.2.zip), extract the zip file, and run MyPubgTelemetry.GUI.exe.
1. Obtain a [PUBG API key](https://developer.playbattlegrounds.com/) and paste it into the program's options screen.
1. The initial telemetry download could take 10+ minutes. Subsequent refreshes will run much more quickly.

### Building from Source
1. Download the repo and open the .sln file in Visual Studio (2019).
1. Build the solution and run the "TelemetryGUI" project. The application window should appear.

### Plans for the Future
1. Add more charts and reports to the GUI, calculated from match metadata
1. Add more charts and reports to the GUI, calculated from match telemetry
1. Make it easier to author/plug-in new reports and/or charts
1. Display what map the match was played on (whaaat?)
1. Handling for disjoint squad players
1. Collect user feedback, implement cool features, get pull requests, do all the things!
