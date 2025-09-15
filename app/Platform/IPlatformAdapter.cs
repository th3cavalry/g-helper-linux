using System.ComponentModel;

namespace GHelper.Platform
{
    /// <summary>
    /// Platform abstraction interface for hardware control and system integration
    /// </summary>
    public interface IPlatformAdapter
    {
        /// <summary>
        /// Initialize the platform adapter
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Check if the platform adapter is connected and functional
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Get the device model name
        /// </summary>
        string GetModel();

        /// <summary>
        /// Set device control value
        /// </summary>
        void DeviceSet(uint control, int value, string name);

        /// <summary>
        /// Get device control value
        /// </summary>
        int DeviceGet(uint control, string name);

        /// <summary>
        /// Initialize system tray or notification area
        /// </summary>
        void InitializeTray();

        /// <summary>
        /// Show settings window/dialog
        /// </summary>
        void ShowSettings();

        /// <summary>
        /// Hide settings window/dialog
        /// </summary>
        void HideSettings();

        /// <summary>
        /// Exit the application
        /// </summary>
        void Exit();

        /// <summary>
        /// Subscribe to power mode changes
        /// </summary>
        event EventHandler<PowerModeChangedEventArgs> PowerModeChanged;

        /// <summary>
        /// Subscribe to user preference changes (theme, etc.)
        /// </summary>
        event EventHandler<EventArgs> UserPreferenceChanged;
    }

    public class PowerModeChangedEventArgs : EventArgs
    {
        public PowerModes Mode { get; set; }
    }

    public enum PowerModes
    {
        Resume,
        StatusChange,
        Suspend
    }
}