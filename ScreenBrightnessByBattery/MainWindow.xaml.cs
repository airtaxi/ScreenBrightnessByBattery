using Microsoft.UI.Xaml;
using Microsoft.Windows.System.Power;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Power;
using WinUIEx;

namespace ScreenBrightnessByBattery;


public sealed partial class MainWindow : Window
{
    // Constants
    private const string SettingsDefaultRawBatteryBrightness = "70";
    private const string SettingsDefaultRawAcBrightness = "100";
    private const string SettingsSection = "Brightness";
    private const string SettingsKeyBattery = "Battery";
    private const string SettingsKeyAc = "AC";

    // Path to the settings file
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.IsShownInSwitchers = false;

        var battery = Battery.AggregateBattery;
        battery.ReportUpdated += OnBatteryReportUpdated;

        if(!File.Exists(SettingsPath))
        {
            var result = IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyBattery, SettingsDefaultRawBatteryBrightness);
            result = IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyAc, SettingsDefaultRawAcBrightness);
        }

        UpdateStartupProcessMenuFlyoutItemText();
        ApplySettings();
    }

    // Indicates if the last time we checked the power supply status, we were on battery or not
    // Null means we haven't checked yet
    private static bool? s_wasOnBattery;
    private static SemaphoreSlim s_applyBrightnessSemaphre = new(1, 1);

    /// <summary>
    /// Applies the brightness settings based on the current power supply status
    /// Also saves the current brightness settings to the settings file
    /// </summary>
    private static async void ApplySettings()
    {
        await s_applyBrightnessSemaphre.WaitAsync();
        try
        {
            var status = PowerManager.PowerSupplyStatus;
            SaveCurrentBrightnessSettings();

            if (status == PowerSupplyStatus.NotPresent)
            {
                // (Widows Bug?) This method is called multiple times when device is on battery
                if (s_wasOnBattery == true) return;

                // (Widows bug?) Setting brigntness immediately after getting brightness is glitchy
                await Task.Delay(1000);

                // Get the brightness from the settings file
                var rawBrightness = IniFile.GetValue(SettingsPath, SettingsSection, SettingsKeyBattery, SettingsDefaultRawBatteryBrightness);
                var success = int.TryParse(rawBrightness, out int brightness);
                if (!success) brightness = int.Parse(SettingsDefaultRawBatteryBrightness);

                // Apply the brightness settings
                Debug.WriteLine($"Applying battery brightness {brightness}");
                WindowsSettingsBrightnessController.Set(brightness);
                s_wasOnBattery = true;
            }
            else if (status == PowerSupplyStatus.Adequate || status == PowerSupplyStatus.Inadequate)
            {
                // (Widows Bug?) This method is called multiple times when device is on AC
                if (s_wasOnBattery == false) return;

                // (Widows bug?) Setting brigntness immediately after getting brightness is glitchy
                await Task.Delay(100);

                // Get the brightness settings from the settings file
                var rawBrightness = IniFile.GetValue(SettingsPath, SettingsSection, SettingsKeyAc, SettingsDefaultRawAcBrightness);
                var success = int.TryParse(rawBrightness, out int brightness);
                if (!success) brightness = int.Parse(SettingsDefaultRawAcBrightness);

                // Apply the brightness settings
                Debug.WriteLine($"Applying AC brightness {brightness}");
                WindowsSettingsBrightnessController.Set(brightness);
                s_wasOnBattery = false;
            }
        }
        finally { s_applyBrightnessSemaphre.Release(); }
    }

    /// <summary>
    /// Saves the current brightness settings to the settings file
    /// </summary>
    private static void SaveCurrentBrightnessSettings()
    {
        // Save the current brightness if we were on battery
        if (s_wasOnBattery == true)
        {
            var currentBrightness = WindowsSettingsBrightnessController.Get();
            Debug.WriteLine($"Saving battery brightness {currentBrightness}");
            IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyBattery, currentBrightness.ToString());
        }
        // Save the current brightness if we were on AC
        else if (s_wasOnBattery == false)
        {
            var currentBrightness = WindowsSettingsBrightnessController.Get();
            Debug.WriteLine($"Saving AC brightness {currentBrightness}");
            IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyAc, currentBrightness.ToString());
        }
    }

    /// <summary>
    /// Updates the text of the Startup Process Menu Flyout Item
    /// </summary>
    private void UpdateStartupProcessMenuFlyoutItemText()
    {
        var menuFlyoutItem = MfiStartupProcess;
        menuFlyoutItem.Text = StartupProcessHelper.IsStartupProcess ? "Remove from Startup Process" : "Add to Startup Process";
    }

    private void OnBatteryReportUpdated(Battery sender, object args) => ApplySettings();

    // Menu Flyout Item Click Handlers
    private void OnOpenSettingsFileMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start("notepad.exe", SettingsPath);
    private void OnApplySettingsNowMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => ApplySettings();
    private void OnExitMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Environment.Exit(0);
    private void OnStartupProcessFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        if (StartupProcessHelper.IsStartupProcess) StartupProcessHelper.RemoveStartupProcess();
        else StartupProcessHelper.SetupStartupProcess();
        UpdateStartupProcessMenuFlyoutItemText();
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args) => this.Hide();
}
