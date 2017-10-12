@ECHO OFF

IF "%CONFIGURATION%"=="" SET CONFIGURATION=Debug

star --resourcedir="%~dp0src\UserAdmin\wwwroot" "%~dp0src/UserAdmin/bin/%CONFIGURATION%/UserAdmin.exe"
