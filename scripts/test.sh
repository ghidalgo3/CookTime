#!/bin/bash
set -e

echo "Running tests..."
cd Tests
dotnet test --no-restore --verbosity normal
