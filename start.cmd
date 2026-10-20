@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0dev.ps1" %*
if errorlevel 1 (
  echo.
  echo Startup failed. Press any key to close.
  pause >nul
)
