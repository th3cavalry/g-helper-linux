using System.Diagnostics;
using System.Runtime.InteropServices;
using Tmds.DBus;

namespace GHelper.Platform
{
    /// <summary>
    /// Linux platform adapter using asus-linux tools (asusctl, supergfxctl)
    /// </summary>
    public class LinuxPlatformAdapter : IPlatformAdapter
    {
        private bool _isInitialized = false;
        private bool _isConnected = false;
        private string _model = "";

        public event EventHandler<PowerModeChangedEventArgs>? PowerModeChanged;
        public event EventHandler<EventArgs>? UserPreferenceChanged;

        public bool Initialize()
        {
            try
            {
                // Check if we're running on Linux
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return false;
                }

                // Check if asusctl is available
                var asusctlCheck = ExecuteCommand("which", "asusctl");
                if (asusctlCheck.exitCode != 0)
                {
                    Console.WriteLine("asusctl not found. Please install asus-linux tools.");
                    return false;
                }

                // Check if supergfxctl is available (optional)
                var supergfxCheck = ExecuteCommand("which", "supergfxctl");
                if (supergfxCheck.exitCode == 0)
                {
                    Console.WriteLine("supergfxctl found");
                }

                // Get model information
                var modelResult = ExecuteCommand("asusctl", "-v");
                if (modelResult.exitCode == 0)
                {
                    _model = ExtractModelFromOutput(modelResult.output);
                }

                _isInitialized = true;
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Linux platform adapter: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        public string GetModel()
        {
            return _model;
        }

        public void DeviceSet(uint control, int value, string name)
        {
            try
            {
                // Map ASUS ACPI controls to asusctl commands
                switch (control)
                {
                    case AsusACPI.PerformanceMode:
                        SetPerformanceMode(value);
                        break;
                    case AsusACPI.GPUModeControl:
                        SetGPUMode(value);
                        break;
                    case AsusACPI.BatteryLimit:
                        SetBatteryLimit(value);
                        break;
                    case AsusACPI.Brightness:
                        SetKeyboardBrightness(value);
                        break;
                    default:
                        Console.WriteLine($"Unsupported control: {control} with value: {value}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set device control {name}: {ex.Message}");
            }
        }

        public int DeviceGet(uint control, string name)
        {
            try
            {
                // Map ASUS ACPI controls to asusctl queries
                switch (control)
                {
                    case AsusACPI.PerformanceMode:
                        return GetPerformanceMode();
                    case AsusACPI.GPUModeControl:
                        return GetGPUMode();
                    case AsusACPI.BatteryLimit:
                        return GetBatteryLimit();
                    case AsusACPI.Brightness:
                        return GetKeyboardBrightness();
                    default:
                        Console.WriteLine($"Unsupported control: {control}");
                        return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get device control {name}: {ex.Message}");
                return -1;
            }
        }

        public void InitializeTray()
        {
            // For now, this is a placeholder
            // In a full implementation, this would create a system tray icon
            Console.WriteLine("Tray initialized (placeholder)");
        }

        public void ShowSettings()
        {
            // For now, this is a placeholder
            // In a full implementation, this would show the GUI
            Console.WriteLine("Settings shown (placeholder)");
        }

        public void HideSettings()
        {
            // For now, this is a placeholder
            Console.WriteLine("Settings hidden (placeholder)");
        }

        public void Exit()
        {
            Environment.Exit(0);
        }

        #region Private Helper Methods

        private (int exitCode, string output) ExecuteCommand(string command, string arguments)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Command error: {error}");
                }

                return (process.ExitCode, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to execute command {command} {arguments}: {ex.Message}");
                return (-1, "");
            }
        }

        private string ExtractModelFromOutput(string output)
        {
            // Extract model from asusctl version output
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Device:") || line.Contains("Model:"))
                {
                    return line.Split(':').LastOrDefault()?.Trim() ?? "Unknown";
                }
            }
            return "Unknown ASUS Device";
        }

        private void SetPerformanceMode(int mode)
        {
            string modeStr = mode switch
            {
                0 => "Balanced",
                1 => "Performance", 
                2 => "Quiet",
                _ => "Balanced"
            };

            var result = ExecuteCommand("asusctl", $"profile -P {modeStr}");
            if (result.exitCode != 0)
            {
                Console.WriteLine($"Failed to set performance mode to {modeStr}");
            }
        }

        private int GetPerformanceMode()
        {
            var result = ExecuteCommand("asusctl", "profile -p");
            if (result.exitCode == 0)
            {
                if (result.output.Contains("Quiet")) return 2;
                if (result.output.Contains("Performance")) return 1;
                return 0; // Balanced
            }
            return -1;
        }

        private void SetGPUMode(int mode)
        {
            // Check if supergfxctl is available
            var checkResult = ExecuteCommand("which", "supergfxctl");
            if (checkResult.exitCode != 0)
            {
                Console.WriteLine("supergfxctl not available for GPU mode control");
                return;
            }

            string modeStr = mode switch
            {
                0 => "Integrated", // Eco
                1 => "Hybrid",     // Standard
                2 => "Vfio",       // Ultimate
                _ => "Hybrid"
            };

            var result = ExecuteCommand("supergfxctl", $"-m {modeStr}");
            if (result.exitCode != 0)
            {
                Console.WriteLine($"Failed to set GPU mode to {modeStr}");
            }
        }

        private int GetGPUMode()
        {
            var checkResult = ExecuteCommand("which", "supergfxctl");
            if (checkResult.exitCode != 0)
            {
                return -1;
            }

            var result = ExecuteCommand("supergfxctl", "-g");
            if (result.exitCode == 0)
            {
                if (result.output.Contains("Integrated")) return 0;
                if (result.output.Contains("Vfio")) return 2;
                return 1; // Hybrid
            }
            return -1;
        }

        private void SetBatteryLimit(int limit)
        {
            var result = ExecuteCommand("asusctl", $"bios -c {limit}");
            if (result.exitCode != 0)
            {
                Console.WriteLine($"Failed to set battery limit to {limit}");
            }
        }

        private int GetBatteryLimit()
        {
            var result = ExecuteCommand("asusctl", "bios -c");
            if (result.exitCode == 0)
            {
                // Parse the output to extract battery limit
                var lines = result.output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("battery_charge_limit"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int limit))
                        {
                            return limit;
                        }
                    }
                }
            }
            return -1;
        }

        private void SetKeyboardBrightness(int brightness)
        {
            var result = ExecuteCommand("asusctl", $"led-pow -b {brightness}");
            if (result.exitCode != 0)
            {
                Console.WriteLine($"Failed to set keyboard brightness to {brightness}");
            }
        }

        private int GetKeyboardBrightness()
        {
            var result = ExecuteCommand("asusctl", "led-pow");
            if (result.exitCode == 0)
            {
                // Parse the output to extract brightness
                var lines = result.output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("brightness"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int brightness))
                        {
                            return brightness;
                        }
                    }
                }
            }
            return -1;
        }

        #endregion
    }

    // Define the ACPI constants we need to map
    public static class AsusACPI
    {
        public const uint PerformanceMode = 0x00120075;
        public const uint GPUModeControl = 0x00090020;
        public const uint BatteryLimit = 0x00120057;
        public const uint Brightness = 0x00050021;
    }
}