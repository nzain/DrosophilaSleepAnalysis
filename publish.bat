@ECHO OFF

SET prjDir=%~dp0src\DroSleep.App
SET prj=%prjDir%\DroSleep.App.csproj
SET ini=%prjDir%\DroSleep.ini
SET deployDir=%~dp0Release
SET deployWin64=%deployDir%\win-x64
SET deployLinux64=%deployDir%\linux-x64

ECHO publish to: %deployDir%

IF EXIST "%deployDir%" rmdir /s /q "%deployDir%"

dotnet publish -c Release --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true^
 -r win-x64^
 -o "%deployWin64%" "%prj%"
 
dotnet publish -c Release --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true^
 -r linux-x64^
 -o "%deployLinux64%" "%prj%"

xcopy /d /s "%ini%" "%deployDir%"

REM .\windows-x64.warp-packer.exe^
REM 	--arch windows-x64^
REM 	--input_dir "%deployWin64%"^
REM 	--exec DroSleep.exe^
REM 	--output "%deployDir%\DroSleep.exe"
REM 	
REM .\windows-x64.warp-packer.exe^
REM 	--arch linux-x64^
REM 	--input_dir "%deployLinux64%"^
REM 	--exec DroSleep^
REM 	--output "%deployDir%\DroSleep_linux"

PAUSE