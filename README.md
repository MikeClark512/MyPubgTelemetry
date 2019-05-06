# MyPubgTelemetry
This project is new and in development. Using it will require Visual Studio, editing code, and recompiling.

## How to use
1. Download the repo and open the .sln file in Visual Studio (2019).
1. Edit MyPubgTelemetry\Program.cs to change the username you want to download telemetry for.
1. Run the project (e.g. Debug (menu) -> Run)
1. Paste your [PUBG API key](https://developer.playbattlegrounds.com/) into the command window when prompted.
1. Telemetry data for the user's matches is downloaded and saved to %APPDATA%\MyPubgTelemetry\telemetry_files\
1. Telemetry data that has already been downloaded will be skipped.
1. Old telemetry data that is no longer available online will be preserved in the download directory.

## Plans for the Future
1. Read usernames from config or command-line instead of hard-coded
1. Add GUI Telemetry Analyzer for analyzing saved telemetry data
