## PUBG Telemetry Downloader and Analyzer

![main](https://github.com/MikeClark512/MyPubgTelemetry/blob/master/site/screenshots/main.png)

This app downloads and saves all available telemetry and match metadata for one or more players.
The downloader skips files that you have already downloaded.
Previously downloaded telemetry that is no longer available online is preserved, so that you can accumulate historical data over time.

### Download and Use
1. This project is new and in development. Expect some bugs.
1. Download [MyPubgTelemetry-v0.0.1.zip](https://github.com/MikeClark512/MyPubgTelemetry/releases/download/v0.0.1/MyPubgTelemetry-v0.0.1.zip), extract the zip file, and run MyPubgTelemetry.GUI.exe.
1. Obtain a [PUBG API key](https://developer.playbattlegrounds.com/) and paste it into the program's options screen.
1. The initial telemetry download could take a long time. Subsequent reloads will run much more quickly.

### Building from Source
1. Download the repo and open the .sln file in Visual Studio (2019).
1. Run the "TelemetryGUI" project. The application window should appear.

### Plans for the Future
1. Add more charts and reports to the GUI
1. Add individual-player statistics instead of just squad-summed values
1. Make it easier to author/plug-in new reports and/or charts

### Projects Structure
1. `TelemetryGUI` - GUI-based telemetry downloader and analyzer.
1. `TelemetryApp` - Contains code and classes shared between all solution projects
1. `TelemetryDownloader` - Console-based telemetry downloader and cacher
1. Any other projects in the solution are works-in-progress and may come and go
