#!/bin/bash

# G-Helper Linux Build Script
# Build script for the Linux port of G-Helper

set -e  # Exit on any error

echo "==================================="
echo "G-Helper Linux Build Script"
echo "==================================="

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET 8 SDK is not installed."
    echo "Please install .NET 8 SDK:"
    echo "  Ubuntu/Debian: sudo apt install dotnet-sdk-8.0"
    echo "  Fedora: sudo dnf install dotnet-sdk-8.0"
    echo "  Arch: sudo pacman -S dotnet-sdk"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "Using .NET version: $DOTNET_VERSION"

# Navigate to the linux-app directory
if [ ! -d "linux-app" ]; then
    echo "Error: linux-app directory not found."
    echo "Please run this script from the repository root directory."
    exit 1
fi

cd linux-app

echo "Building G-Helper Linux..."

# Clean previous builds
echo "Cleaning previous builds..."
dotnet clean --verbosity quiet

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore --verbosity quiet

# Build release version
echo "Building release version..."
dotnet build --configuration Release --verbosity minimal

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Build successful!"
    echo ""
    echo "To run G-Helper Linux:"
    echo "  cd linux-app"
    echo "  dotnet run --configuration Release"
    echo ""
    echo "Or create a standalone executable:"
    echo "  dotnet publish --configuration Release --self-contained --runtime linux-x64"
    echo "  ./bin/Release/net8.0/linux-x64/publish/GHelperLinux"
    echo ""
    echo "For usage instructions, run:"
    echo "  dotnet run --configuration Release -- --help"
else
    echo "❌ Build failed!"
    exit 1
fi

echo "==================================="