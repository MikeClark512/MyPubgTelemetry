# MyPubgTelemetry

Use `TelemetryDownloader` to download and save all of the available telemetry for one or more players. For efficiency, the downloader will skip telemetry files that you have already downloaded. It also will not delete old telemetry that is no longer available online, so that you can aggregate historical data for yourself.

Use `TelemetryGUI` to view charts and reported calculated from saved telemetry.  Use `TelemetryAnalyzer` to generate text reports.

## How to use
1. This project is new and in development. Using it will require Visual Studio, editing code, and recompiling.
1. Download the repo and open the .sln file in Visual Studio (2019).
1. Edit MyPubgTelemetry\Program.cs to change the username you want to download telemetry for.
1. Run the project (e.g. Debug (menu) -> Run)
1. Paste your [PUBG API key](https://developer.playbattlegrounds.com/) into the command window when prompted.
1. Telemetry data for the user's matches is downloaded and saved to %APPDATA%\MyPubgTelemetry\telemetry_files\
1. Telemetry data that has already been downloaded will be skipped.
1. Old telemetry data that is no longer available online will be preserved in the download directory.

## Plans for the Future
1. Read usernames from config or command-line instead of hard-coded
1. Add more charts and reports to the GUI
1. Make it easier to plugin new reports to the GUI

## Projects Structure
1. `TelemetryApp` - Contains code and classes shared between all projects
1. `TelemetryDownloader` - Console-based telemetry downloader and cacher
1. `TelemetryGUI` - GUI-based telemetry analyzer
1. `TelemetryAnalyzer` - Console-based telemetry analyzer
