#!/bin/bash
set -e

OUTPUT_PATH=${1:-"./publish"}

echo "Publishing to $OUTPUT_PATH..."
cd src
dotnet publish --configuration Release --verbosity normal --output "$OUTPUT_PATH"
