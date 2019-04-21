@ECHO OFF

SET prjDir=%~dp0src\DroSleep.App
SET prj=%prjDir%\DroSleep.App.csproj
SET ini=%prjDir%\DroSleep.ini
SET deployDir=%~dp0Release
SET deployWin64=%deployDir%\win-x64
SET deployLinux64=%deployDir%\linux-x64

ECHO publish to: %deployDir%

IF EXIST "%deployDir%" rmdir /s /q "%deployDir%"

dotnet publish -c Release -r win-x64 -o "%deployWin64%" "%prj%"
dotnet publish -c Release -r linux-x64 -o "%deployLinux64%" "%prj%"
xcopy /d /s "%ini%" "%deployDir%"

.\windows-x64.warp-packer.exe^
	--arch windows-x64^
	--input_dir "%deployWin64%"^
	--exec DroSleep.exe^
	--output "%deployDir%\DroSleep.exe"
	
.\windows-x64.warp-packer.exe^
	--arch linux-x64^
	--input_dir "%deployLinux64%"^
	--exec DroSleep^
	--output "%deployDir%\DroSleep_linux"

PAUSE