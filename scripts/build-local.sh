#!/bin/bash

# Quick local build for immediate use
# Creates a single-file executable for your current platform

set -e

echo "🔨 Building Sandra.Snow v2 for local use..."

# Detect current platform
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    RUNTIME="win-x64"
    OUTPUT_EXT=".exe"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    if [[ $(uname -m) == "arm64" ]]; then
        RUNTIME="osx-arm64"
    else
        RUNTIME="osx-x64"
    fi
    OUTPUT_EXT=""
else
    if [[ $(uname -m) == "aarch64" ]]; then
        RUNTIME="linux-arm64"
    else
        RUNTIME="linux-x64"
    fi
    OUTPUT_EXT=""
fi

OUTPUT_DIR="./dist"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

echo "📦 Building for platform: $RUNTIME"

# Build single-file executable
dotnet publish src/Snow.Cli/Snow.Cli.csproj \
    --configuration Release \
    --runtime "$RUNTIME" \
    --self-contained true \
    --output "$OUTPUT_DIR" \
    /p:PublishSingleFile=true \
    /p:IncludeNativeLibrariesForSelfExtract=true \
    /p:PublishTrimmed=false

echo ""
echo "✅ Build complete!"
echo "📁 Executable location: $OUTPUT_DIR/Snow.Cli$OUTPUT_EXT"
echo "📏 File size: $(du -h "$OUTPUT_DIR/Snow.Cli$OUTPUT_EXT" | cut -f1)"
echo ""
echo "🎯 Test the build:"
echo "   $OUTPUT_DIR/Snow.Cli$OUTPUT_EXT --version"
echo "   $OUTPUT_DIR/Snow.Cli$OUTPUT_EXT --help"
echo ""
echo "🚀 You can now copy this executable anywhere and use it!"
