## PUBG Telemetry Downloader and Analyzer

![main](https://github.com/MikeClark512/MyPubgTelemetry/blob/master/site/screenshots/main.png)

This app downloads and saves all available telemetry and match metadata for one or more players.
The downloader skips files that you have already downloaded.
Previously downloaded telemetry that is no longer available online is preserved, so that you can accumulate historical data over time.

### Requirements
This project is new and in development.
Using it will require Visual Studio and recompiling.
Release binary builds are planned and will be available in the future.

### Instructions
1. Download the repo and open the .sln file in Visual Studio (2019).
1. Run the "TelemetryGUI" project. The application window should appear.
1. Open the options screen.
1. Paste your [PUBG API key](https://developer.playbattlegrounds.com/) into the API Key field.
1. Close the options screen by pressing OK.
1. Enter your name (and optionally a comma separated list of your friend's names) into the squad field
1. Press the reload button.
1. The initial download could take a long time. Subsequent reloads will run much more quickly.

### Plans for the Future
1. Add more charts and reports to the GUI
1. Add more individual-player statistics
1. Make it easier to author/plug-in new reports and/or charts
1. CSV Export

### Projects Structure
1. `TelemetryGUI` - GUI-based telemetry downloader and analyzer
1. `TelemetryApp` - Contains code and classes shared between all projects
1. `TelemetryDownloader` - Console-based telemetry downloader and cacher
1. Any other projects in the solution are works-in-progress and may come and go
