@echo off
setlocal

set pn=MyPubgTelemetry-v0.0.1
set stage=pkg\stage\%pn%
set dist=pkg\dist\%pn%

if exist pkg rd /s /q pkg
mkdir %stage%
mkdir %dist%

xcopy /q .\TelemetryGUI\bin\Debug\*.exe %stage%
xcopy /q .\TelemetryGUI\bin\Debug\*.exe.config %stage%
xcopy /qi .\TelemetryGUI\bin\Debug\*.dll %stage%\lib
