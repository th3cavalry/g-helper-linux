# G-Helper for Linux

A lightweight Linux port of G-Helper that integrates with asus-linux tools to provide system control for ASUS laptops.

## Overview

This is a Linux port of the popular G-Helper tool for ASUS laptops. Instead of using Windows ACPI/WMI interfaces, this version leverages the excellent [asus-linux](https://asus-linux.org/) project tools to provide similar functionality on Linux systems.

## Features

- **Modern GUI Interface**: Avalonia-based graphical interface with tabbed design similar to Windows version
- **Performance Mode Control**: Switch between Quiet, Balanced, and Performance modes
- **GPU Mode Control**: Switch between Integrated, Hybrid, and Vfio modes (requires supergfxctl)
- **Battery Charge Limiting**: Set battery charge limits to preserve battery health  
- **Keyboard Brightness Control**: Adjust keyboard backlight brightness
- **System Status Monitoring**: View current system configuration
- **Console Interface**: Command-line interface for scripting and remote access
- **Interactive Mode**: Real-time control through console commands
- **Cross-platform .NET 8**: Built with modern .NET for excellent Linux compatibility

## Prerequisites

This tool requires the asus-linux tools to be installed on your system:

### Installation by Distribution

**Ubuntu/Debian:**
```bash
sudo apt install asusctl supergfxctl
```

**Fedora:**
```bash
sudo dnf install asusctl supergfxctl
```

**Arch Linux:**
```bash
sudo pacman -S asusctl supergfxctl
```

**Other distributions:**
Visit [asus-linux.org](https://asus-linux.org/) for installation instructions.

### Verification

After installation, verify the tools are working:
```bash
asusctl --version
supergfxctl --help  # Optional, for GPU switching
```

## Installation

### Option 1: Build from Source

1. Ensure you have .NET 8 SDK installed:
   ```bash
   # Ubuntu/Debian
   sudo apt install dotnet-sdk-8.0
   
   # Fedora  
   sudo dnf install dotnet-sdk-8.0
   
   # Arch
   sudo pacman -S dotnet-sdk
   ```

2. Clone and build:
   ```bash
   git clone https://github.com/th3cavalry/g-helper-linux.git
   cd g-helper-linux/linux-app
   dotnet build --configuration Release
   ```

3. Run:
   ```bash
   dotnet run --configuration Release
   ```

### Option 2: Create Standalone Executable

```bash
cd g-helper-linux/linux-app
dotnet publish --configuration Release --self-contained --runtime linux-x64
./bin/Release/net8.0/linux-x64/publish/GHelperLinux
```

## Usage

### Command Line Mode

```bash
# Show system status
ghelper-linux --status

# Set performance mode
ghelper-linux --performance Quiet
ghelper-linux --performance Balanced  
ghelper-linux --performance Performance

# Set GPU mode (requires supergfxctl)
ghelper-linux --gpu Integrated
ghelper-linux --gpu Hybrid
ghelper-linux --gpu Vfio

# Set battery charge limit
ghelper-linux --battery 80

# Set keyboard brightness (0-3)
ghelper-linux --keyboard 2

# List available profiles
ghelper-linux --profiles

# Show help
ghelper-linux --help
```

### Interactive Mode

Run without arguments to enter interactive mode:

```bash
ghelper-linux
```

This provides a command-line interface where you can type commands:

```
ghelper> status
ghelper> performance Quiet
ghelper> battery 85
ghelper> help
ghelper> quit
```

## Supported Hardware

This tool works with any ASUS laptop supported by the asus-linux project. This includes most modern ASUS gaming laptops and ultrabooks such as:

- ROG Zephyrus series (G14, G15, G16, etc.)
- ROG Strix series  
- TUF Gaming series
- Zenbook series
- VivoBook series
- ProArt series

For a complete compatibility list, see [asus-linux.org](https://asus-linux.org/).

## Architecture

The Linux port replaces Windows-specific components as follows:

| Windows G-Helper | Linux G-Helper |
|------------------|----------------|
| ASUS ACPI/WMI | asusctl commands |
| WinForms GUI | Console interface |
| Windows Services | systemd/direct execution |
| Registry settings | Config files |
| NVIDIA API | GPU drivers + supergfxctl |

## Development

The codebase is structured for easy maintenance and extension:

- `LinuxAsusControl.cs`: Core hardware control using asus-linux tools
- `Program.cs`: Command-line interface and interactive mode
- `GHelperLinux.csproj`: .NET project configuration

### Contributing

1. Fork the repository
2. Create a feature branch
3. Test with your ASUS hardware
4. Submit a pull request

## Comparison with Windows G-Helper

| Feature | Windows G-Helper | Linux G-Helper | Status |
|---------|------------------|----------------|--------|
| Performance Modes | ✅ | ✅ | Complete |
| GPU Switching | ✅ | ✅ | Complete |
| Battery Limits | ✅ | ✅ | Complete |  
| Keyboard Backlight | ✅ | ✅ | Complete |
| GUI Interface | ✅ | ✅ | **Complete** |
| System Tray | ✅ | ⏳ | Planned |
| Fan Curves | ✅ | ⏳ | Planned |
| Anime Matrix | ✅ | ⏳ | Planned |

## New GUI Interface

G-Helper for Linux now includes a modern graphical interface built with Avalonia UI that provides the same functionality as the Windows version:

### GUI Features
- **Modern Design**: Fluent theme with tabbed interface
- **Performance Control**: Easy dropdown selection for Quiet/Balanced/Performance modes
- **GPU Management**: Switch between Integrated/Hybrid/Vfio modes with visual feedback
- **Battery Health**: Slider control for charge limits (20-100%) with real-time preview
- **Keyboard Lighting**: Brightness control with immediate feedback
- **System Status**: Live display of current system configuration
- **Cross-Platform**: Same UI framework works across Linux distributions

### Starting the GUI

The GUI launches automatically when you run G-Helper:

```bash
# Default behavior - launches GUI
dotnet run --framework net8.0

# Or explicitly request GUI
dotnet run --framework net8.0 -- settings
```

### GUI Screenshots

The GUI provides a familiar tabbed interface similar to the Windows version:

- **Performance Tab**: Control CPU performance and fan behavior
- **Battery Tab**: Set charge limits to preserve battery health  
- **Keyboard Tab**: Adjust backlight brightness levels
- **Status Tab**: View current system configuration

### Fallback Options

If GUI cannot start (no display available), G-Helper automatically falls back to:

```bash
# Console mode with interactive commands
dotnet run --framework net8.0 -- console

# Demo mode for testing (no hardware required)
dotnet run --framework net8.0 -- --mock
```

## Limitations

- No graphical interface (console-only)
- Requires asus-linux tools installation
- Some advanced features not yet implemented
- Requires elevated permissions for some operations

## Troubleshooting

### "asusctl not found" Error

Install the asus-linux tools for your distribution. See the Prerequisites section.

### Permission Denied Errors

Some operations may require elevated permissions:
```bash
sudo ghelper-linux --battery 80
```

### Command Not Working

Verify asusctl is working directly:
```bash
asusctl profile -l
asusctl bios -c
```

## License

This project maintains the same license as the original G-Helper project.

## Acknowledgments

- Original G-Helper by [seerge](https://github.com/seerge/g-helper)
- [asus-linux project](https://asus-linux.org/) for the excellent Linux support tools
- The Linux community for testing and feedback

## Links

- [Original G-Helper](https://github.com/seerge/g-helper)
- [asus-linux Project](https://asus-linux.org/)
- [asusctl Documentation](https://gitlab.com/asus-linux/asusctl)
- [supergfxctl Documentation](https://gitlab.com/asus-linux/supergfxctl)