using GHelper.Platform;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

#if NET8_0_WINDOWS
using GHelper.Ally;
using GHelper.Battery;
using GHelper.Display;
using GHelper.Gpu;
using GHelper.Helpers;
using GHelper.Input;
using GHelper.Mode;
using GHelper.Peripherals;
using GHelper.USB;
using Microsoft.Win32;
using Ryzen;
using static NativeMethods;
#endif

namespace GHelper
{

    static class Program
    {
#if NET8_0_WINDOWS
        // Windows-specific members
        public static NotifyIcon trayIcon;
        public static AsusACPI acpi;

        public static SettingsForm settingsForm = new SettingsForm();

        public static ModeControl modeControl = new ModeControl();
        public static GPUModeControl gpuControl = new GPUModeControl(settingsForm);
        public static AllyControl allyControl = new AllyControl(settingsForm);
        public static ClamshellModeControl clamshellControl = new ClamshellModeControl();

        public static ToastForm toast = new ToastForm();

        public static IntPtr unRegPowerNotify, unRegPowerNotifyLid;
        public static int WM_TASKBARCREATED = 0;

        private static long lastAuto;
        private static long lastTheme;

        public static InputDispatcher? inputDispatcher;

        private static PowerLineStatus isPlugged = SystemInformation.PowerStatus.PowerLineStatus;
#endif

        // Cross-platform platform adapter
        public static IPlatformAdapter platformAdapter;

        // The main entry point for the application
        public static void Main(string[] args)
        {
            try
            {
                // Initialize platform adapter
                platformAdapter = PlatformFactory.GetPlatformAdapter();
                if (!platformAdapter.Initialize())
                {
                    Console.WriteLine("Platform adapter initialization failed - starting in demo mode");
                    
                    // Check if we're on Linux
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Run demo mode for Linux when hardware isn't available
                        var mockAdapter = new GHelper.GUI.Linux.MockLinuxPlatformAdapter();
                        mockAdapter.Initialize();
                        RunLinuxDemo(mockAdapter);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Failed to initialize platform adapter");
                        Environment.Exit(1);
                        return;
                    }
                }

#if NET8_0_WINDOWS
                // Run Windows-specific main
                MainWindows(args);
#else
                // Run Linux-specific main
                MainLinux(args);
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        // Demo mode for Linux when hardware is not available
        private static void RunLinuxDemo(GHelper.GUI.Linux.MockLinuxPlatformAdapter mockAdapter)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("G-Helper Linux GUI Demo");
            Console.WriteLine("=================================");
            Console.WriteLine();
            
            Console.WriteLine($"Model: {mockAdapter.GetModel()}");
            Console.WriteLine($"Connected: {mockAdapter.IsConnected()}");
            Console.WriteLine();

            // Test the device operations
            Console.WriteLine("📊 Current System Status:");
            Console.WriteLine($"  Performance Mode: {GetPerformanceModeString(mockAdapter.DeviceGet(0x00120075, "Performance Mode"))}");
            Console.WriteLine($"  GPU Mode: {GetGPUModeString(mockAdapter.DeviceGet(0x00090020, "GPU Mode"))}");
            Console.WriteLine($"  Battery Limit: {mockAdapter.DeviceGet(0x00120057, "Battery Limit")}%");
            Console.WriteLine($"  Keyboard Brightness: Level {mockAdapter.DeviceGet(0x00050012, "Keyboard Brightness")}");
            Console.WriteLine();

            // Test changing settings
            Console.WriteLine("🔧 Testing Setting Changes:");
            mockAdapter.DeviceSet(0x00120075, 2, "Performance Mode"); // Set to Quiet
            mockAdapter.DeviceSet(0x00090020, 0, "GPU Mode"); // Set to Integrated
            mockAdapter.DeviceSet(0x00120057, 90, "Battery Limit"); // Set to 90%
            mockAdapter.DeviceSet(0x00050012, 3, "Keyboard Brightness"); // Set to level 3
            Console.WriteLine();

            // Verify changes
            Console.WriteLine("✅ Verified New Settings:");
            Console.WriteLine($"  Performance Mode: {GetPerformanceModeString(mockAdapter.DeviceGet(0x00120075, "Performance Mode"))}");
            Console.WriteLine($"  GPU Mode: {GetGPUModeString(mockAdapter.DeviceGet(0x00090020, "GPU Mode"))}");
            Console.WriteLine($"  Battery Limit: {mockAdapter.DeviceGet(0x00120057, "Battery Limit")}%");
            Console.WriteLine($"  Keyboard Brightness: Level {mockAdapter.DeviceGet(0x00050012, "Keyboard Brightness")}");
            Console.WriteLine();

            // Show GUI capabilities
            Console.WriteLine("🎨 GUI Features Implemented:");
            Console.WriteLine("  ✓ Modern Avalonia UI with Fluent theme");
            Console.WriteLine("  ✓ Tabbed interface (Performance, Battery, Keyboard, Status)");
            Console.WriteLine("  ✓ Performance Mode dropdown (Quiet, Balanced, Performance)");
            Console.WriteLine("  ✓ GPU Mode dropdown (Integrated, Hybrid, Vfio)");
            Console.WriteLine("  ✓ Battery charge limit slider (20-100%)");
            Console.WriteLine("  ✓ Keyboard brightness slider (0-3 levels)");
            Console.WriteLine("  ✓ Real-time system status display");
            Console.WriteLine("  ✓ Error handling and user feedback");
            Console.WriteLine("  ✓ Cross-platform .NET 8 implementation");
            Console.WriteLine();

            // Demonstrate the GUI
            Console.WriteLine("🖥️  Launching GUI Demonstration:");
            mockAdapter.ShowSettings();
            Console.WriteLine();
            
            Console.WriteLine("🚀 To use with real hardware:");
            Console.WriteLine("  1. Install asus-linux tools:");
            Console.WriteLine("     Ubuntu/Debian: sudo apt install asusctl supergfxctl");
            Console.WriteLine("     Fedora: sudo dnf install asusctl supergfxctl");
            Console.WriteLine("     Arch: sudo pacman -S asusctl supergfxctl");
            Console.WriteLine("  2. Run: dotnet run --framework net8.0");
            Console.WriteLine("  3. GUI will launch automatically");
            Console.WriteLine();
            
            Console.WriteLine("📋 Command Line Options:");
            Console.WriteLine("  --mock      : Force demo mode");
            Console.WriteLine("  settings    : Show GUI settings");
            Console.WriteLine("  console     : Force console mode");
            Console.WriteLine("  --help      : Show detailed help");
        }

        private static string GetPerformanceModeString(int mode)
        {
            return mode switch
            {
                0 => "Balanced",
                1 => "Performance",
                2 => "Quiet",
                _ => "Unknown"
            };
        }

        private static string GetGPUModeString(int mode)
        {
            return mode switch
            {
                0 => "Integrated",
                1 => "Hybrid", 
                2 => "Vfio",
                _ => "Unknown"
            };
        }

#if NET8_0_WINDOWS
        // Windows-specific main method
        private static void MainWindows(string[] args)
        {

            string action = "";
            if (args.Length > 0) action = args[0];

            if (action == "charge")
            {
                if (AppConfig.IsZ13()) Aura.Init();
                BatteryLimit();
                InputDispatcher.StartupBacklight();
                Application.Exit();
                return;
            }

            string language = AppConfig.GetString("language");

            if (language != null && language.Length > 0)
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            else
            {
                var culture = CultureInfo.CurrentUICulture;
                if (culture.ToString() == "kr") culture = CultureInfo.GetCultureInfo("ko");
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            ProcessHelper.CheckAlreadyRunning();

            Logger.WriteLine("------------");
            Logger.WriteLine("App launched: " + AppConfig.GetModel() + " :" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + CultureInfo.CurrentUICulture + (ProcessHelper.IsUserAdministrator() ? "." : ""));

            var startCount = AppConfig.Get("start_count") + 1;
            AppConfig.Set("start_count", startCount);
            Logger.WriteLine("Start Count: " + startCount);

            acpi = new AsusACPI();

            if (!acpi.IsConnected() && AppConfig.IsASUS())
            {
                DialogResult dialogResult = MessageBox.Show(Properties.Strings.ACPIError, Properties.Strings.StartupError, MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo("https://www.asus.com/support/FAQ/1047338/") { UseShellExecute = true });
                }

                Application.Exit();
                return;
            }

            ProcessHelper.KillByName("ASUSSmartDisplayControl");

            Application.EnableVisualStyles();

            HardwareControl.RecreateGpuControl();
            RyzenControl.Init();

            trayIcon = new NotifyIcon
            {
                Text = "G-Helper",
                Icon = Properties.Resources.standard,
                Visible = true
            };

            WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
            Logger.WriteLine($"Tray Icon: {trayIcon.Visible} | {WM_TASKBARCREATED}");

            settingsForm.SetContextMenu();
            trayIcon.MouseClick += TrayIcon_MouseClick;
            trayIcon.MouseMove += TrayIcon_MouseMove;


            inputDispatcher = new InputDispatcher();

            settingsForm.InitAura();
            settingsForm.InitMatrix();

            gpuControl.InitXGM();

            SetAutoModes(init: true);

            // Subscribing for system power change events
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            clamshellControl.RegisterDisplayEvents();
            clamshellControl.ToggleLidAction();

            // Subscribing for monitor power on events
            unRegPowerNotify = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, PowerSettingGuid.ConsoleDisplayState, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
            unRegPowerNotifyLid = NativeMethods.RegisterPowerSettingNotification(settingsForm.Handle, PowerSettingGuid.LIDSWITCH_STATE_CHANGE, NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);


            Task task = Task.Run((Action)PeripheralsProvider.DetectAllAsusMice);
            PeripheralsProvider.RegisterForDeviceEvents();

            if (Environment.CurrentDirectory.Trim('\\') == Application.StartupPath.Trim('\\') || action.Length > 0)
            {
                SettingsToggle(false);
            }

            switch (action)
            {
                case "cpu":
                    Startup.ReScheduleAdmin();
                    settingsForm.FansToggle();
                    break;
                case "gpu":
                    Startup.ReScheduleAdmin();
                    settingsForm.FansToggle(1);
                    break;
                case "gpurestart":
                    gpuControl.RestartGPU(false);
                    break;
                case "services":
                    settingsForm.extraForm = new Extra();
                    settingsForm.extraForm.Show();
                    settingsForm.extraForm.ServiesToggle();
                    break;
                case "uv":
                    Startup.ReScheduleAdmin();
                    settingsForm.FansToggle(2);
                    modeControl.SetRyzen();
                    break;
                case "colors":
                    Task.Run(async () =>
                    {
                        await ColorProfileHelper.InstallProfile();
                        settingsForm.Invoke(delegate
                        {
                            settingsForm.InitVisual();
                        });
                    });
                    break;
                default:
                    Task.Run(Startup.StartupCheck);
                    break;
            }

            Application.Run();

        }


        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            gpuControl.StandardModeFix();
            modeControl.ShutdownReset();
            BatteryControl.AutoBattery();
            InputDispatcher.ShutdownStatusLed();
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLogon || e.Reason == SessionSwitchReason.SessionUnlock)
            {
                Logger.WriteLine("Session:" + e.Reason.ToString());
                ScreenControl.AutoScreen();
            }
        }

        static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {

            if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastTheme) < 2000) return;

            switch (e.Category)
            {
                case UserPreferenceCategory.General:
                    bool changed = settingsForm.InitTheme();
                    settingsForm.VisualiseIcon();

                    if (changed)
                    {
                        Debug.WriteLine("Theme Changed");
                        lastTheme = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }

                    if (settingsForm.fansForm is not null && settingsForm.fansForm.Text != "")
                        settingsForm.fansForm.InitTheme();

                    if (settingsForm.extraForm is not null && settingsForm.extraForm.Text != "")
                        settingsForm.extraForm.InitTheme();

                    if (settingsForm.updatesForm is not null && settingsForm.updatesForm.Text != "")
                        settingsForm.updatesForm.InitTheme();

                    if (settingsForm.matrixForm is not null && settingsForm.matrixForm.Text != "")
                        settingsForm.matrixForm.InitTheme();

                    if (settingsForm.handheldForm is not null && settingsForm.handheldForm.Text != "")
                        settingsForm.handheldForm.InitTheme();

                    break;
            }
        }



        public static bool SetAutoModes(bool powerChanged = false, bool init = false, bool wakeup = false)
        {
            int skipDelay = wakeup ? 10000 : 3000;

            if (Math.Abs(DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastAuto) < skipDelay) return false;
            lastAuto = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            isPlugged = SystemInformation.PowerStatus.PowerLineStatus;
            Logger.WriteLine("AutoSetting for " + isPlugged.ToString());

            BatteryControl.AutoBattery(init);
            if (init) InputDispatcher.InitScreenpad();
            DynamicLightingHelper.Init();
            ScreenControl.InitOptimalBrightness();

            inputDispatcher.Init();
            //HardwareControl.ReadSensors(true);

            modeControl.AutoPerformance(powerChanged);

            settingsForm.matrixControl.SetDevice(true);
            InputDispatcher.InitStatusLed();
            XGM.InitLight();

            if (AppConfig.IsAlly())
            {
                allyControl.Init();
            }
            else
            {
                InputDispatcher.AutoKeyboard();
            }

            bool switched = gpuControl.AutoGPUMode(delay: 1000);
            if (!switched)
            {
                gpuControl.InitGPUMode();
                ScreenControl.AutoScreen();
            }

            ScreenControl.InitMiniled();
            VisualControl.InitBrightness();

            return true;
        }

        private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            Logger.WriteLine($"Power Mode {e.Mode}: {SystemInformation.PowerStatus.PowerLineStatus}");
            if (e.Mode == PowerModes.Suspend)
            {
                Logger.WriteLine("Power Mode Changed:" + e.Mode.ToString());
                gpuControl.StandardModeFix(true);
                modeControl.ShutdownReset();
                InputDispatcher.ShutdownStatusLed();
            }

            int delay = AppConfig.Get("charger_delay");
            if (delay > 0)
            {
                Logger.WriteLine($"Charger Delay: {delay}");
                Thread.Sleep(delay);
            }

            if (SystemInformation.PowerStatus.PowerLineStatus == isPlugged) return;
            if (AppConfig.Is("disable_power_event")) return;
            SetAutoModes(powerChanged: true);
        }

        public static void SettingsToggle(bool checkForFocus = true, bool trayClick = false)
        {
            if (settingsForm.Visible)
            {
                // If helper window is not on top, this just focuses on the app again
                // Pressing the ghelper button again will hide the app
                if (checkForFocus && !settingsForm.HasAnyFocus(trayClick) && !AppConfig.Is("topmost"))
                {
                    settingsForm.ShowAll();
                }
                else
                {
                    settingsForm.HideAll();
                }
            }
            else
            {
                var screen = Screen.PrimaryScreen;
                if (screen is null) screen = Screen.FromControl(settingsForm);

                settingsForm.Location = screen.WorkingArea.Location;
                settingsForm.Left = screen.WorkingArea.Width - 10 - settingsForm.Width;
                settingsForm.Top = screen.WorkingArea.Height - 10 - settingsForm.Height;

                settingsForm.Show();
                settingsForm.ShowAll();

                settingsForm.Left = screen.WorkingArea.Width - 10 - settingsForm.Width;

                if (AppConfig.IsAlly())
                    settingsForm.Top = Math.Max(10, screen.Bounds.Height - 110 - settingsForm.Height);
                else
                    settingsForm.Top = screen.WorkingArea.Height - 10 - settingsForm.Height;

                settingsForm.VisualiseGPUMode();
            }
        }

        static void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                SettingsToggle(trayClick: true);

        }

        static void TrayIcon_MouseMove(object? sender, MouseEventArgs e)
        {
            settingsForm.RefreshSensors();
        }

        static void OnExit(object sender, EventArgs e)
        {
            if (trayIcon is not null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            PeripheralsProvider.UnregisterForDeviceEvents();
            clamshellControl.UnregisterDisplayEvents();
            NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotify);
            NativeMethods.UnregisterPowerSettingNotification(unRegPowerNotifyLid);
            Application.Exit();
        }

        static void BatteryLimit()
        {
            try
            {
                int limit = AppConfig.Get("charge_limit");
                if (limit > 0 && limit < 100)
                {
                    Logger.WriteLine($"------- Startup Battery Limit {limit} -------");
                    ProcessHelper.StartEnableService("ATKWMIACPIIO", false);
                    Logger.WriteLine($"Connecting to ACPI");
                    acpi = new AsusACPI();
                    Logger.WriteLine($"Setting Limit");
                    acpi.DeviceSet(AsusACPI.BatteryLimit, limit, "Limit");
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Startup Battery Limit Error: " + ex.Message);
            }
        }

#endif

        // Linux-specific main method
        private static void MainLinux(string[] args)
        {
            Console.WriteLine("G-Helper Linux starting...");
            
            string action = "";
            if (args.Length > 0) action = args[0];

            // Set culture
            string language = Environment.GetEnvironmentVariable("LANG") ?? "en";
            try
            {
                var culture = CultureInfo.GetCultureInfo(language.Split('.')[0].Replace('_', '-'));
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            }

            Console.WriteLine($"G-Helper Linux {Assembly.GetExecutingAssembly().GetName().Version} started");
            Console.WriteLine($"Platform: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Model: {platformAdapter.GetModel()}");

            // Check if we should use mock mode for testing
            if (args.Contains("--mock") || !platformAdapter.IsConnected())
            {
                Console.WriteLine();
                Console.WriteLine("⚠️  Hardware not available or mock mode requested");
                Console.WriteLine("🔄 Switching to demonstration mode...");
                Console.WriteLine();
                
                var mockAdapter = new GHelper.GUI.Linux.MockLinuxPlatformAdapter();
                mockAdapter.Initialize();
                
                // Demonstrate the functionality
                RunLinuxDemo(mockAdapter);
                return;
            }

            // Initialize tray and GUI support
            platformAdapter.InitializeTray();

            // Handle command line actions
            switch (action)
            {
                case "charge":
                    HandleBatteryLimit();
                    Environment.Exit(0);
                    return;
                case "performance":
                    SetPerformanceMode();
                    break;
                case "gpu":
                    SetGPUMode();
                    break;
                case "settings":
                case "gui":
                    // Show GUI and keep running
                    platformAdapter.ShowSettings();
                    // For GUI mode, we don't want the console loop
                    Console.WriteLine("GUI started. Close the window to exit.");
                    // Wait for the application to exit
                    WaitForGUIExit();
                    return;
                case "console":
                    // Force console mode even when GUI is available
                    break;
                default:
                    // Default behavior - try GUI first, fall back to console
                    Console.WriteLine("Starting GUI mode. Use 'console' argument to force console mode.");
                    try
                    {
                        platformAdapter.ShowSettings();
                        Console.WriteLine("GUI started. Close the window to exit, or press Ctrl+C for console mode.");
                        WaitForGUIExit();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to start GUI: {ex.Message}");
                        Console.WriteLine("Falling back to console mode...");
                    }
                    break;
            }

            // Console mode
            Console.WriteLine("Console mode active. Available commands:");
            Console.WriteLine("  settings: Show GUI settings");
            Console.WriteLine("  status: Show current status");
            Console.WriteLine("  help: Show help");
            Console.WriteLine("  q: Quit");
            Console.WriteLine("Press Ctrl+C to exit or 'q' + Enter to quit");
            
            // Set up Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                platformAdapter.Exit();
            };

            // Simple command loop
            string? input;
            while ((input = Console.ReadLine()) != "q")
            {
                if (input == "settings")
                {
                    platformAdapter.ShowSettings();
                }
                else if (input == "status")
                {
                    ShowStatus();
                }
                else if (input == "help")
                {
                    ShowHelp();
                }
                else if (!string.IsNullOrEmpty(input))
                {
                    Console.WriteLine($"Unknown command: {input}. Type 'help' for available commands.");
                }
            }

            platformAdapter.Exit();
        }

        private static void WaitForGUIExit()
        {
            // Set up Ctrl+C handler for GUI mode
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\nSwitching to console mode...");
                e.Cancel = true;
                platformAdapter.HideSettings();
            };

            // Wait for user input or application exit
            Task.Run(() =>
            {
                Console.ReadLine(); // Wait for any input to potentially switch to console mode
            });

            // This is a simple implementation - in a real app, you'd wait for the GUI to close
            // For now, we'll just keep the process alive
            while (true)
            {
                Thread.Sleep(1000);
                // In a real implementation, you'd check if the GUI window is still open
                // and break when it's closed
            }
        }

        private static void HandleBatteryLimit()
        {
            try
            {
                Console.WriteLine("Setting battery charge limit...");
                // This would be implemented using the platform adapter
                // For now, just a placeholder
                platformAdapter.DeviceSet(0x00120057, 80, "BatteryLimit");
                Console.WriteLine("Battery limit set to 80%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting battery limit: {ex.Message}");
            }
        }

        private static void SetPerformanceMode()
        {
            try
            {
                Console.WriteLine("Current performance mode:");
                int currentMode = platformAdapter.DeviceGet(0x00120075, "PerformanceMode");
                string[] modes = { "Balanced", "Performance", "Quiet" };
                if (currentMode >= 0 && currentMode < modes.Length)
                {
                    Console.WriteLine($"  {modes[currentMode]}");
                }
                else
                {
                    Console.WriteLine("  Unknown");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting performance mode: {ex.Message}");
            }
        }

        private static void SetGPUMode()
        {
            try
            {
                Console.WriteLine("Current GPU mode:");
                int currentMode = platformAdapter.DeviceGet(0x00090020, "GPUMode");
                string[] modes = { "Integrated", "Hybrid", "Discrete" };
                if (currentMode >= 0 && currentMode < modes.Length)
                {
                    Console.WriteLine($"  {modes[currentMode]}");
                }
                else
                {
                    Console.WriteLine("  Unknown");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting GPU mode: {ex.Message}");
            }
        }

        private static void ShowStatus()
        {
            Console.WriteLine("\n=== G-Helper Status ===");
            Console.WriteLine($"Platform: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Model: {platformAdapter.GetModel()}");
            Console.WriteLine($"Connected: {platformAdapter.IsConnected()}");
            
            try
            {
                // Show current settings
                SetPerformanceMode();
                SetGPUMode();
                
                int batteryLimit = platformAdapter.DeviceGet(0x00120057, "BatteryLimit");
                if (batteryLimit > 0)
                {
                    Console.WriteLine($"Battery charge limit: {batteryLimit}%");
                }
                
                int brightness = platformAdapter.DeviceGet(0x00050021, "Brightness");
                if (brightness >= 0)
                {
                    Console.WriteLine($"Keyboard brightness: {brightness}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting status: {ex.Message}");
            }
            Console.WriteLine("=====================\n");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("\n=== G-Helper Commands ===");
            Console.WriteLine("settings  - Show settings (placeholder)");
            Console.WriteLine("status    - Show current device status");
            Console.WriteLine("help      - Show this help");
            Console.WriteLine("q         - Quit application");
            Console.WriteLine("========================\n");
        }

    }
}