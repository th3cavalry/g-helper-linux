#if NET8_0_WINDOWS
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace GHelper.Platform
{
    /// <summary>
    /// Windows platform adapter wrapping existing AsusACPI functionality
    /// </summary>
    public class WindowsPlatformAdapter : IPlatformAdapter
    {
        private AsusACPI _acpi;
        private System.Windows.Forms.NotifyIcon _trayIcon;
        private SettingsForm _settingsForm;

        public event EventHandler<PowerModeChangedEventArgs>? PowerModeChanged;
        public event EventHandler<EventArgs>? UserPreferenceChanged;

        public bool Initialize()
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return false;
                }

                _acpi = new AsusACPI();
                return _acpi.IsConnected();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Windows platform adapter: {ex.Message}");
                return false;
            }
        }

        public bool IsConnected()
        {
            return _acpi?.IsConnected() ?? false;
        }

        public string GetModel()
        {
            return AppConfig.GetModel();
        }

        public void DeviceSet(uint control, int value, string name)
        {
            _acpi?.DeviceSet(control, value, name);
        }

        public int DeviceGet(uint control, string name)
        {
            return _acpi?.DeviceGet(control, name) ?? -1;
        }

        public void InitializeTray()
        {
            _trayIcon = new System.Windows.Forms.NotifyIcon
            {
                Text = "G-Helper",
                Icon = Properties.Resources.standard,
                Visible = true
            };

            _trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        public void ShowSettings()
        {
            if (_settingsForm == null)
            {
                _settingsForm = new SettingsForm();
            }
            
            Program.SettingsToggle(false);
        }

        public void HideSettings()
        {
            Program.SettingsToggle(false);
        }

        public void Exit()
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            System.Windows.Forms.Application.Exit();
        }

        private void TrayIcon_MouseClick(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                ShowSettings();
        }

        public void SubscribeToSystemEvents()
        {
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            var mode = e.Mode switch
            {
                Microsoft.Win32.PowerModes.Resume => PowerModes.Resume,
                Microsoft.Win32.PowerModes.StatusChange => PowerModes.StatusChange,
                Microsoft.Win32.PowerModes.Suspend => PowerModes.Suspend,
                _ => PowerModes.StatusChange
            };

            PowerModeChanged?.Invoke(this, new PowerModeChangedEventArgs { Mode = mode });
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            UserPreferenceChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
#endif