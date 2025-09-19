using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GHelper.Platform;
using System;
using System.Threading.Tasks;

namespace GHelper.GUI.Linux
{
    public partial class MainWindow : Window
    {
        private readonly IPlatformAdapter _platformAdapter;
        
        // UI Controls
        private ComboBox? _performanceModeCombo;
        private ComboBox? _gpuModeCombo;
        private Slider? _batteryLimitSlider;
        private TextBlock? _batteryLimitLabel;
        private Button? _applyBatteryLimitButton;
        private Slider? _keyboardBrightnessSlider;
        private TextBlock? _keyboardBrightnessLabel;
        private TextBlock? _modelLabel;
        private TextBlock? _statusModelLabel;
        private TextBlock? _statusPerformanceLabel;
        private TextBlock? _statusGPULabel;
        private TextBlock? _statusBatteryLabel;
        private Button? _refreshStatusButton;
        private Button? _quitButton;

        public MainWindow()
        {
            InitializeComponent();
            _platformAdapter = PlatformFactory.GetPlatformAdapter();
            
            this.Opened += MainWindow_Opened;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            // Get references to UI controls
            _performanceModeCombo = this.FindControl<ComboBox>("PerformanceModeCombo");
            _gpuModeCombo = this.FindControl<ComboBox>("GPUModeCombo");
            _batteryLimitSlider = this.FindControl<Slider>("BatteryLimitSlider");
            _batteryLimitLabel = this.FindControl<TextBlock>("BatteryLimitLabel");
            _applyBatteryLimitButton = this.FindControl<Button>("ApplyBatteryLimitButton");
            _keyboardBrightnessSlider = this.FindControl<Slider>("KeyboardBrightnessSlider");
            _keyboardBrightnessLabel = this.FindControl<TextBlock>("KeyboardBrightnessLabel");
            _modelLabel = this.FindControl<TextBlock>("ModelLabel");
            _statusModelLabel = this.FindControl<TextBlock>("StatusModelLabel");
            _statusPerformanceLabel = this.FindControl<TextBlock>("StatusPerformanceLabel");
            _statusGPULabel = this.FindControl<TextBlock>("StatusGPULabel");
            _statusBatteryLabel = this.FindControl<TextBlock>("StatusBatteryLabel");
            _refreshStatusButton = this.FindControl<Button>("RefreshStatusButton");
            _quitButton = this.FindControl<Button>("QuitButton");
            
            // Wire up event handlers
            if (_performanceModeCombo != null)
                _performanceModeCombo.SelectionChanged += PerformanceModeCombo_SelectionChanged;
            
            if (_gpuModeCombo != null)
                _gpuModeCombo.SelectionChanged += GPUModeCombo_SelectionChanged;
            
            if (_batteryLimitSlider != null)
                _batteryLimitSlider.PropertyChanged += BatteryLimitSlider_PropertyChanged;
            
            if (_applyBatteryLimitButton != null)
                _applyBatteryLimitButton.Click += ApplyBatteryLimitButton_Click;
            
            if (_keyboardBrightnessSlider != null)
                _keyboardBrightnessSlider.PropertyChanged += KeyboardBrightnessSlider_PropertyChanged;
            
            if (_refreshStatusButton != null)
                _refreshStatusButton.Click += RefreshStatusButton_Click;
            
            if (_quitButton != null)
                _quitButton.Click += QuitButton_Click;
        }

        private async void MainWindow_Opened(object? sender, EventArgs e)
        {
            await LoadInitialData();
        }

        private Task LoadInitialData()
        {
            try
            {
                // Load model information
                var model = _platformAdapter.GetModel();
                if (_modelLabel != null)
                    _modelLabel.Text = !string.IsNullOrEmpty(model) ? model : "ASUS Laptop";

                // Load current settings
                return RefreshAllStatus();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial data: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        private Task RefreshAllStatus()
        {
            try
            {
                // Update performance mode
                var perfMode = GetCurrentPerformanceMode();
                if (_performanceModeCombo != null && !string.IsNullOrEmpty(perfMode))
                {
                    for (int i = 0; i < _performanceModeCombo.ItemCount; i++)
                    {
                        if (_performanceModeCombo.Items[i] is ComboBoxItem item && 
                            item.Tag?.ToString() == perfMode)
                        {
                            _performanceModeCombo.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Update GPU mode
                var gpuMode = GetCurrentGPUMode();
                if (_gpuModeCombo != null && !string.IsNullOrEmpty(gpuMode))
                {
                    for (int i = 0; i < _gpuModeCombo.ItemCount; i++)
                    {
                        if (_gpuModeCombo.Items[i] is ComboBoxItem item && 
                            item.Tag?.ToString() == gpuMode)
                        {
                            _gpuModeCombo.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Update battery limit
                var batteryLimit = GetCurrentBatteryLimit();
                if (_batteryLimitSlider != null && batteryLimit > 0)
                {
                    _batteryLimitSlider.Value = batteryLimit;
                }

                // Update keyboard brightness
                var keyboardBrightness = GetCurrentKeyboardBrightness();
                if (_keyboardBrightnessSlider != null && keyboardBrightness >= 0)
                {
                    _keyboardBrightnessSlider.Value = keyboardBrightness;
                }

                // Update status labels
                if (_statusModelLabel != null)
                    _statusModelLabel.Text = _platformAdapter.GetModel();
                
                if (_statusPerformanceLabel != null)
                    _statusPerformanceLabel.Text = perfMode ?? "Unknown";
                
                if (_statusGPULabel != null)
                    _statusGPULabel.Text = gpuMode ?? "Unknown";
                
                if (_statusBatteryLabel != null)
                    _statusBatteryLabel.Text = batteryLimit > 0 ? $"{batteryLimit}%" : "Not set";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing status: {ex.Message}");
            }
            
            return Task.CompletedTask;
        }

        private string? GetCurrentPerformanceMode()
        {
            try
            {
                // Map platform adapter calls to performance modes
                var result = _platformAdapter.DeviceGet(0x00120075, "Performance Mode");
                return result switch
                {
                    0 => "Balanced",
                    1 => "Performance", 
                    2 => "Quiet",
                    _ => "Balanced"
                };
            }
            catch
            {
                return "Balanced";
            }
        }

        private string? GetCurrentGPUMode()
        {
            try
            {
                // Map platform adapter calls to GPU modes
                var result = _platformAdapter.DeviceGet(0x00090020, "GPU Mode");
                return result switch
                {
                    0 => "Integrated",
                    1 => "Hybrid",
                    2 => "Vfio",
                    _ => "Hybrid"
                };
            }
            catch
            {
                return "Hybrid";
            }
        }

        private int GetCurrentBatteryLimit()
        {
            try
            {
                return _platformAdapter.DeviceGet(0x00120057, "Battery Limit");
            }
            catch
            {
                return 80; // Default
            }
        }

        private int GetCurrentKeyboardBrightness()
        {
            try
            {
                return _platformAdapter.DeviceGet(0x00050012, "Keyboard Brightness");
            }
            catch
            {
                return 1; // Default
            }
        }

        private void PerformanceModeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_performanceModeCombo?.SelectedItem is ComboBoxItem item && item.Tag is string mode)
            {
                try
                {
                    var value = mode switch
                    {
                        "Quiet" => 2,
                        "Balanced" => 0,
                        "Performance" => 1,
                        _ => 0
                    };
                    
                    _platformAdapter.DeviceSet(0x00120075, value, "Performance Mode");
                    Console.WriteLine($"Performance mode set to: {mode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting performance mode: {ex.Message}");
                }
            }
        }

        private void GPUModeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_gpuModeCombo?.SelectedItem is ComboBoxItem item && item.Tag is string mode)
            {
                try
                {
                    var value = mode switch
                    {
                        "Integrated" => 0,
                        "Hybrid" => 1,
                        "Vfio" => 2,
                        _ => 1
                    };
                    
                    _platformAdapter.DeviceSet(0x00090020, value, "GPU Mode");
                    Console.WriteLine($"GPU mode set to: {mode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting GPU mode: {ex.Message}");
                }
            }
        }

        private void BatteryLimitSlider_PropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value" && _batteryLimitLabel != null && _batteryLimitSlider != null)
            {
                _batteryLimitLabel.Text = $"{(int)_batteryLimitSlider.Value}%";
            }
        }

        private void ApplyBatteryLimitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_batteryLimitSlider != null)
            {
                try
                {
                    var limit = (int)_batteryLimitSlider.Value;
                    _platformAdapter.DeviceSet(0x00120057, limit, "Battery Limit");
                    Console.WriteLine($"Battery limit set to: {limit}%");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting battery limit: {ex.Message}");
                }
            }
        }

        private void KeyboardBrightnessSlider_PropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value" && _keyboardBrightnessLabel != null && _keyboardBrightnessSlider != null)
            {
                var level = (int)_keyboardBrightnessSlider.Value;
                _keyboardBrightnessLabel.Text = $"Level {level}";
                
                try
                {
                    _platformAdapter.DeviceSet(0x00050012, level, "Keyboard Brightness");
                    Console.WriteLine($"Keyboard brightness set to: {level}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting keyboard brightness: {ex.Message}");
                }
            }
        }

        private async void RefreshStatusButton_Click(object? sender, RoutedEventArgs e)
        {
            await RefreshAllStatus();
        }

        private void QuitButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // Save any settings if needed
            base.OnClosing(e);
        }
    }
}