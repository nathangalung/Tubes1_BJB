@echo off
REM filepath: /c:/Users/NathanGalung/Documents/Kuliah/sem6/stima/Tubes/IF2211_TB1_BJB/src/alternative-bots/kenan/kenan.cmd

echo Checking for existing output directories...
if exist bin\ (
  echo Cleaning previous build outputs...
  rmdir /s /q bin
)

if exist obj\ (
  echo Cleaning object files...
  rmdir /s /q obj
)

echo Building kenan bot...
dotnet build

echo Starting kenan bot...
dotnet run --no-build

echo Bot execution complete.