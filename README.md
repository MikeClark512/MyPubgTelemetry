# MyPubgTelemetry
This project is new and in development, and as such, not very useful or well-organized. 

## What it does (currently) and how to do it
1. Download the project and open in Visual Studio (2019).
1. Paste your [PUBG API key](https://developer.playbattlegrounds.com/) into data/APIKEY.txt so that the program can use it to access PUBG APIs.
1. You have to edit the source code to change the username. 
1. Debug -> Run Without Debugging.
1. A list of matches is downloaded for the username specified (if the username is found).
1. Metadata for the first match of the specified user is retrieved and printed.
1. Telemetry data for the first match listed is downloaded and saved to a file. The filename is printed to the screen.

## Plans for the Future
Eventually the program will allow incremental (e.g. daily) downloading and accumulation of telemetry data for a list of preset users (e.g. you and your friends). 
Fascinating statistics can be generated from the accumulated statistics.
