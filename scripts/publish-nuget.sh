#!/bin/bash

# Sandra.Snow v2 NuGet Publishing Script
# Publishes packages to NuGet.org

set -e

echo "📦 Publishing Sandra.Snow v2 to NuGet..."

# Build release packages
dotnet build --configuration Release

# Pack all packages
dotnet pack src/Snow.Abstractions/Snow.Abstractions.csproj --configuration Release --output ./packages
dotnet pack src/Snow.Engine/Snow.Engine.csproj --configuration Release --output ./packages
dotnet pack src/Snow.Cli/Snow.Cli.csproj --configuration Release --output ./packages

echo "📁 Packages created:"
ls -la ./packages/*.nupkg

echo ""
echo "🚀 To publish to NuGet.org:"
echo "dotnet nuget push ./packages/*.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY"
echo ""
echo "🎯 Users can then install with:"
echo "dotnet tool install --global Sandra.Snow.Cli"
