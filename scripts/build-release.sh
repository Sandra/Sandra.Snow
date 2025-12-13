#!/bin/bash

# Sandra.Snow v2 Release Build Script
# Creates single-file executables for all platforms

set -e

echo "🚀 Building Sandra.Snow v2 Release Packages..."

# Configuration
OUTPUT_DIR="./release"
PROJECT_PATH="./src/Snow.Cli/Snow.Cli.csproj"
VERSION="2.0.0-beta.1"

# Clean output directory
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Build configurations for each platform
PLATFORMS=(
    "win-x64"
    "win-arm64"
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

echo "📦 Building single-file executables..."

for platform in "${PLATFORMS[@]}"; do
    echo "Building for $platform..."
    
    # Output directory for this platform
    PLATFORM_DIR="$OUTPUT_DIR/$platform"
    mkdir -p "$PLATFORM_DIR"
    
    # Publish single-file executable
    dotnet publish "$PROJECT_PATH" \
        --configuration Release \
        --runtime "$platform" \
        --self-contained true \
        --output "$PLATFORM_DIR" \
        /p:PublishSingleFile=true \
        /p:IncludeNativeLibrariesForSelfExtract=true \
        /p:PublishTrimmed=false \
        /p:DebugType=embedded
    
    # Create archive
    if [[ "$platform" == win-* ]]; then
        # Windows - create ZIP
        cd "$OUTPUT_DIR"
        zip -r "sandra-snow-v$VERSION-$platform.zip" "$platform/"
        cd - > /dev/null
    else
        # Unix - create tar.gz
        cd "$OUTPUT_DIR"
        tar -czf "sandra-snow-v$VERSION-$platform.tar.gz" "$platform/"
        cd - > /dev/null
    fi
    
    echo "✅ $platform complete"
done

echo ""
echo "📦 Building NuGet packages..."

# Build NuGet packages for libraries
dotnet pack ./src/Snow.Abstractions/Snow.Abstractions.csproj --configuration Release --output "$OUTPUT_DIR/nuget"
dotnet pack ./src/Snow.Engine/Snow.Engine.csproj --configuration Release --output "$OUTPUT_DIR/nuget"  
dotnet pack ./src/Snow.Cli/Snow.Cli.csproj --configuration Release --output "$OUTPUT_DIR/nuget"

echo ""
echo "🎊 Release build complete!"
echo "📁 Output directory: $OUTPUT_DIR"
echo ""
echo "📦 Single-file executables:"
ls -la "$OUTPUT_DIR"/*.{zip,tar.gz} 2>/dev/null || true
echo ""
echo "📦 NuGet packages:"
ls -la "$OUTPUT_DIR/nuget"/*.nupkg 2>/dev/null || true
echo ""
echo "🚀 Ready for release!"
