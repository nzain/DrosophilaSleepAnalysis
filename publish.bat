@ECHO OFF

SET prjDir=%~dp0src\DroSleep.App
SET prj=%prjDir%\DroSleep.App.csproj
SET ini=%prjDir%\DroSleep.ini
SET deployDir=%~dp0Release

ECHO publish to: %deployDir%

IF EXIST "%deployDir%" rmdir /s /q "%deployDir%"

dotnet publish -c Release -r win-x64 -o "%deployDir%" "%prj%"
xcopy /d /s "%ini%" "%deployDir%"

PAUSE