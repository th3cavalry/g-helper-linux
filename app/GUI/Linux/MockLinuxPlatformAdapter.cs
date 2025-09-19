using System;
using System.Threading.Tasks;
using GHelper.Platform;

namespace GHelper.GUI.Linux
{
    /// <summary>
    /// Mock platform adapter for testing GUI without actual hardware
    /// </summary>
    public class MockLinuxPlatformAdapter : IPlatformAdapter
    {
        private bool _isInitialized = false;
        private bool _isConnected = true;
        private string _model = "ASUS ROG Zephyrus G14 (Mock)";
        
        private int _performanceMode = 0; // 0=Balanced, 1=Performance, 2=Quiet
        private int _gpuMode = 1; // 0=Integrated, 1=Hybrid, 2=Vfio
        private int _batteryLimit = 80;
        private int _keyboardBrightness = 1;

        public event EventHandler<PowerModeChangedEventArgs>? PowerModeChanged;
        public event EventHandler<EventArgs>? UserPreferenceChanged;

        public bool Initialize()
        {
            Console.WriteLine("Mock: Initializing platform adapter...");
            _isInitialized = true;
            return true;
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
            Console.WriteLine($"Mock: Setting {name} (control: {control:X}) to {value}");
            
            // Simulate the settings changes
            switch (control)
            {
                case 0x00120075: // Performance Mode
                    _performanceMode = value;
                    Console.WriteLine($"Mock: Performance mode set to {GetPerformanceModeString(value)}");
                    break;
                case 0x00090020: // GPU Mode
                    _gpuMode = value;
                    Console.WriteLine($"Mock: GPU mode set to {GetGPUModeString(value)}");
                    break;
                case 0x00120057: // Battery Limit
                    _batteryLimit = value;
                    Console.WriteLine($"Mock: Battery limit set to {value}%");
                    break;
                case 0x00050012: // Keyboard Brightness
                    _keyboardBrightness = value;
                    Console.WriteLine($"Mock: Keyboard brightness set to level {value}");
                    break;
                default:
                    Console.WriteLine($"Mock: Unknown control {control:X}");
                    break;
            }
        }

        public int DeviceGet(uint control, string name)
        {
            var result = control switch
            {
                0x00120075 => _performanceMode,
                0x00090020 => _gpuMode,
                0x00120057 => _batteryLimit,
                0x00050012 => _keyboardBrightness,
                _ => -1
            };
            
            Console.WriteLine($"Mock: Getting {name} (control: {control:X}) = {result}");
            return result;
        }

        public void InitializeTray()
        {
            Console.WriteLine("Mock: Tray initialized with GUI support");
        }

        public void ShowSettings()
        {
            Console.WriteLine("Mock: GUI would launch here showing:");
            Console.WriteLine("  📱 Modern tabbed interface with Fluent theme");
            Console.WriteLine("  ⚡ Performance tab: Dropdown with Quiet/Balanced/Performance modes");
            Console.WriteLine("  🔋 Battery tab: Slider for charge limit (20-100%)");
            Console.WriteLine("  ⌨️ Keyboard tab: Slider for backlight brightness (0-3)");
            Console.WriteLine("  ℹ️ Status tab: Real-time system information display");
            Console.WriteLine("  🎮 Header with ASUS laptop model and branding");
            Console.WriteLine("  ✅ Real-time updates when settings change");
            Console.WriteLine();
            Console.WriteLine("Mock: In a real environment with display, this would launch");
            Console.WriteLine("Mock: the full Avalonia GUI application.");
        }

        public void HideSettings()
        {
            Console.WriteLine("Mock: Hide settings called");
        }

        public void Exit()
        {
            Console.WriteLine("Mock: Exiting application");
            Environment.Exit(0);
        }

        private string GetPerformanceModeString(int mode)
        {
            return mode switch
            {
                0 => "Balanced",
                1 => "Performance",
                2 => "Quiet",
                _ => "Unknown"
            };
        }

        private string GetGPUModeString(int mode)
        {
            return mode switch
            {
                0 => "Integrated",
                1 => "Hybrid", 
                2 => "Vfio",
                _ => "Unknown"
            };
        }
    }
}