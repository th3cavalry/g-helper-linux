#!/bin/bash

# G-Helper Linux Installation Script
# This script helps install G-Helper Linux and its prerequisites

set -e

echo "==========================================="
echo "G-Helper Linux Installation Script"
echo "==========================================="

# Function to detect Linux distribution
detect_distro() {
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        echo "$ID"
    else
        echo "unknown"
    fi
}

# Function to install prerequisites
install_prerequisites() {
    local distro=$(detect_distro)
    
    echo "Detected distribution: $distro"
    echo ""
    
    case "$distro" in
        ubuntu|debian)
            echo "Installing prerequisites for Ubuntu/Debian..."
            echo "This will install: dotnet-sdk-8.0, asusctl, supergfxctl"
            read -p "Do you want to continue? (y/N): " -n 1 -r
            echo ""
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                sudo apt update
                sudo apt install -y dotnet-sdk-8.0 asusctl supergfxctl
            else
                echo "Installation cancelled."
                exit 1
            fi
            ;;
        fedora)
            echo "Installing prerequisites for Fedora..."
            echo "This will install: dotnet-sdk-8.0, asusctl, supergfxctl"
            read -p "Do you want to continue? (y/N): " -n 1 -r
            echo ""
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                sudo dnf install -y dotnet-sdk-8.0 asusctl supergfxctl
            else
                echo "Installation cancelled."
                exit 1
            fi
            ;;
        arch|manjaro)
            echo "Installing prerequisites for Arch Linux..."
            echo "This will install: dotnet-sdk, asusctl, supergfxctl"
            read -p "Do you want to continue? (y/N): " -n 1 -r
            echo ""
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                sudo pacman -S dotnet-sdk asusctl supergfxctl
            else
                echo "Installation cancelled."
                exit 1
            fi
            ;;
        *)
            echo "Unsupported distribution: $distro"
            echo "Please install the following packages manually:"
            echo "- .NET 8 SDK"
            echo "- asusctl"
            echo "- supergfxctl"
            echo ""
            echo "Visit https://asus-linux.org/ for asus-linux tools installation"
            read -p "Do you want to continue building G-Helper anyway? (y/N): " -n 1 -r
            echo ""
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                exit 1
            fi
            ;;
    esac
}

# Function to build G-Helper
build_ghelper() {
    echo ""
    echo "Building G-Helper Linux..."
    
    if [ ! -f "build-linux.sh" ]; then
        echo "Error: build-linux.sh not found. Are you in the correct directory?"
        exit 1
    fi
    
    ./build-linux.sh
}

# Function to create a system-wide installation
install_system_wide() {
    echo ""
    echo "Creating standalone executable..."
    
    cd linux-app
    dotnet publish --configuration Release --self-contained --runtime linux-x64 --output publish
    
    echo ""
    read -p "Do you want to install G-Helper system-wide to /usr/local/bin? (y/N): " -n 1 -r
    echo ""
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        sudo cp publish/GHelperLinux /usr/local/bin/ghelper-linux
        sudo chmod +x /usr/local/bin/ghelper-linux
        echo "✅ G-Helper Linux installed to /usr/local/bin/ghelper-linux"
        echo ""
        echo "You can now run 'ghelper-linux' from anywhere!"
        echo "Try: ghelper-linux --help"
    else
        echo "Standalone executable created in: linux-app/publish/GHelperLinux"
        echo "You can run it with: ./linux-app/publish/GHelperLinux"
    fi
}

# Main installation flow
echo "This script will:"
echo "1. Install prerequisites (.NET 8, asusctl, supergfxctl)"
echo "2. Build G-Helper Linux"
echo "3. Optionally install system-wide"
echo ""

read -p "Do you want to install prerequisites? (Y/n): " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Nn]$ ]]; then
    install_prerequisites
fi

# Check if .NET is available
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is still not available. Please install it manually."
    exit 1
fi

build_ghelper
install_system_wide

echo ""
echo "==========================================="
echo "Installation completed successfully!"
echo ""
echo "Next steps:"
echo "1. Verify asus-linux tools: asusctl --version"
echo "2. Run G-Helper: ghelper-linux --help"
echo "3. Check status: ghelper-linux --status"
echo "==========================================="