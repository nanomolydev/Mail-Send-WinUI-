cd %~dp0
takeown /F server-win.exe
icacls server-win.exe /grant Users:F
takeown /F chrome-win64 /R /D Y
icacls "chrome-win64" /q /c /t /grant Users:F
server-win.exe
pause