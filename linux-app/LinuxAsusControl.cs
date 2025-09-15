using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GHelperLinux
{
    /// <summary>
    /// Linux platform adapter using asus-linux tools (asusctl, supergfxctl)
    /// </summary>
    public class LinuxAsusControl
    {
        private bool _isConnected = false;
        private string _model = "";

        public bool Initialize()
        {
            try
            {
                // Check if we're running on Linux
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine("Not running on Linux");
                    return false;
                }

                // Check if asusctl is available
                var asusctlCheck = ExecuteCommand("which", "asusctl");
                if (asusctlCheck.exitCode != 0)
                {
                    Console.WriteLine("asusctl not found. Please install asus-linux tools:");
                    Console.WriteLine("  https://asus-linux.org/");
                    return false;
                }

                // Check if supergfxctl is available (optional)
                var supergfxCheck = ExecuteCommand("which", "supergfxctl");
                if (supergfxCheck.exitCode == 0)
                {
                    Console.WriteLine("supergfxctl found - GPU switching available");
                }
                else
                {
                    Console.WriteLine("supergfxctl not found - GPU switching not available");
                }

                // Get model information
                var modelResult = ExecuteCommand("asusctl", "--version");
                if (modelResult.exitCode == 0)
                {
                    _model = ExtractModelFromOutput(modelResult.output);
                }

                _isConnected = true;
                Console.WriteLine("LinuxAsusControl initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Linux ASUS control: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected() => _isConnected;
        public string GetModel() => _model;

        public void SetPerformanceMode(string mode)
        {
            var result = ExecuteCommand("asusctl", $"profile -P {mode}");
            if (result.exitCode == 0)
            {
                Console.WriteLine($"Performance mode set to: {mode}");
            }
            else
            {
                Console.WriteLine($"Failed to set performance mode to {mode}: {result.output}");
            }
        }

        public string GetPerformanceMode()
        {
            var result = ExecuteCommand("asusctl", "profile -p");
            if (result.exitCode == 0)
            {
                var lines = result.output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Active profile"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            return "Unknown";
        }

        public void SetGPUMode(string mode)
        {
            var checkResult = ExecuteCommand("which", "supergfxctl");
            if (checkResult.exitCode != 0)
            {
                Console.WriteLine("supergfxctl not available for GPU mode control");
                return;
            }

            var result = ExecuteCommand("supergfxctl", $"-m {mode}");
            if (result.exitCode == 0)
            {
                Console.WriteLine($"GPU mode set to: {mode}");
            }
            else
            {
                Console.WriteLine($"Failed to set GPU mode to {mode}: {result.output}");
            }
        }

        public string GetGPUMode()
        {
            var checkResult = ExecuteCommand("which", "supergfxctl");
            if (checkResult.exitCode != 0)
            {
                return "Not available";
            }

            var result = ExecuteCommand("supergfxctl", "-g");
            if (result.exitCode == 0)
            {
                return result.output.Trim();
            }
            return "Unknown";
        }

        public void SetBatteryLimit(int limit)
        {
            var result = ExecuteCommand("asusctl", $"bios -c {limit}");
            if (result.exitCode == 0)
            {
                Console.WriteLine($"Battery charge limit set to: {limit}%");
            }
            else
            {
                Console.WriteLine($"Failed to set battery limit to {limit}%: {result.output}");
            }
        }

        public int GetBatteryLimit()
        {
            var result = ExecuteCommand("asusctl", "bios -c");
            if (result.exitCode == 0)
            {
                var lines = result.output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("charge_limit"))
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

        public void SetKeyboardBrightness(int brightness)
        {
            var result = ExecuteCommand("asusctl", $"led-pow -b {brightness}");
            if (result.exitCode == 0)
            {
                Console.WriteLine($"Keyboard brightness set to: {brightness}");
            }
            else
            {
                Console.WriteLine($"Failed to set keyboard brightness to {brightness}: {result.output}");
            }
        }

        public int GetKeyboardBrightness()
        {
            var result = ExecuteCommand("asusctl", "led-pow");
            if (result.exitCode == 0)
            {
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

        public void ListAvailableProfiles()
        {
            var result = ExecuteCommand("asusctl", "profile -l");
            if (result.exitCode == 0)
            {
                Console.WriteLine("Available performance profiles:");
                Console.WriteLine(result.output);
            }
            else
            {
                Console.WriteLine("Failed to list profiles");
            }
        }

        public void ShowSystemInfo()
        {
            Console.WriteLine("\n=== ASUS System Information ===");
            Console.WriteLine($"Model: {GetModel()}");
            Console.WriteLine($"Performance Mode: {GetPerformanceMode()}");
            Console.WriteLine($"GPU Mode: {GetGPUMode()}");
            
            int batteryLimit = GetBatteryLimit();
            if (batteryLimit > 0)
            {
                Console.WriteLine($"Battery Charge Limit: {batteryLimit}%");
            }
            
            int brightness = GetKeyboardBrightness();
            if (brightness >= 0)
            {
                Console.WriteLine($"Keyboard Brightness: {brightness}");
            }
            Console.WriteLine("===============================\n");
        }

        #region Helper Methods

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

                // Combine output and error for better debugging
                string combinedOutput = string.IsNullOrEmpty(error) ? output : $"{output}\nError: {error}";

                return (process.ExitCode, combinedOutput.Trim());
            }
            catch (Exception ex)
            {
                return (-1, $"Exception: {ex.Message}");
            }
        }

        private string ExtractModelFromOutput(string output)
        {
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("asusctl") && line.Contains("version"))
                {
                    return "ASUS Linux Device (asusctl detected)";
                }
            }
            return "Unknown ASUS Device";
        }

        #endregion
    }
}