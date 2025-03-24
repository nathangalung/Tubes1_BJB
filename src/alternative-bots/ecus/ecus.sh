#!/bin/bash
# filepath: /c:/Users/NathanGalung/Documents/Kuliah/sem6/stima/Tubes/IF2211_TB1_BJB/src/alternative-bots/ecus/ecus.sh

echo "Checking for existing output directories..."
if [ -d "bin" ]; then
  echo "Cleaning previous build outputs..."
  rm -rf bin
fi

if [ -d "obj" ]; then
  echo "Cleaning object files..."
  rm -rf obj
fi

echo "Building ecus bot..."
dotnet build

echo "Starting ecus bot..."
dotnet run --no-build

echo "Bot execution complete."