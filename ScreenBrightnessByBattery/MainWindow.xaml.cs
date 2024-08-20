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

    /// <summary>
    /// Path to the settings file
    /// </summary>
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

    /// <summary>
    /// 1500ms is adequate interval.
    /// Change it if you want
    /// </summary>
    private static readonly System.Timers.Timer SaveCurrentBrightnessSettingsTimer = new(1500);

    static MainWindow()
    {
        SaveCurrentBrightnessSettingsTimer.Elapsed += async (s, e) =>
        {
            await ApplySettingsAsync();
            SaveCurrentBrightnessSettings();
        };
        SaveCurrentBrightnessSettingsTimer.Start();
    }

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.IsShownInSwitchers = false;

        if(!File.Exists(SettingsPath))
        {
            var result = IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyBattery, SettingsDefaultRawBatteryBrightness);
            result = IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyAc, SettingsDefaultRawAcBrightness);
        }

        UpdateStartupProcessMenuFlyoutItemText();
    }

    /// <summary>
    /// Indicates if the last time we checked the power supply status, we were on battery or not
    /// Null means we haven't checked yet
    /// </summary>
    private static bool? s_wasOnBattery;

    /// <summary>
    /// Applies the brightness settings based on the current power supply status
    /// Also saves the current brightness settings to the settings file
    /// </summary>
    private static async Task ApplySettingsAsync()
    {
        if (IsOnBattery)
        {
            // If we were on battery last time, do nothing
            if (s_wasOnBattery == true) return;

            // Get the brightness from the settings file
            var rawBrightness = IniFile.GetValue(SettingsPath, SettingsSection, SettingsKeyBattery, SettingsDefaultRawBatteryBrightness);

            // Apply the brightness settings if raw brightness is auto
            if (rawBrightness == "auto")
            {
                Debug.WriteLine("Applying battery brightness: auto");
                SetAdaptiveBrightness(true);
                s_wasOnBattery = true;
                return;
            }
            // Turn off adaptive brightness if it is enabled
            else
            {
                var isAdaptiveBrightnessEnabled = GetAdaptiveBrightnessStatus(false);
                if (isAdaptiveBrightnessEnabled == true) SetAdaptiveBrightness(false);
            }

            // Otherwise, apply the brightness settings
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(SettingsDefaultRawBatteryBrightness);

            // Apply the brightness settings
            Debug.WriteLine($"Applying battery brightness {brightness}");

            // Bugfix: Set different two brightness values to force the brightness to change
            // (Maybe on surface devices. My device has this issue)
            if (brightness > 50) PowerConfigBrightnessController.Set(25);
            else PowerConfigBrightnessController.Set(75);

            // Wait for a while to let the brightness change (500ms is adequate)
            await Task.Delay(500);

            // Set the actual brightness
            PowerConfigBrightnessController.Set(brightness);
            s_wasOnBattery = true;
        }
        else
        {
            // If we were on AC last time, do nothing
            if (s_wasOnBattery == false) return;

            // Get the brightness settings from the settings file
            var rawBrightness = IniFile.GetValue(SettingsPath, SettingsSection, SettingsKeyAc, SettingsDefaultRawAcBrightness);
            // Apply the brightness settings if raw brightness is auto
            if (rawBrightness == "auto")
            {
                Debug.WriteLine("Applying AC brightness: auto");
                SetAdaptiveBrightness(true);
                s_wasOnBattery = false;
                return;
            }
            // Turn off adaptive brightness if it is enabled
            else
            {
                var isAdaptiveBrightnessEnabled = GetAdaptiveBrightnessStatus(true);
                if (isAdaptiveBrightnessEnabled == true) SetAdaptiveBrightness(false);
            }

            // Otherwise, apply the brightness settings
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(SettingsDefaultRawAcBrightness);

            // Apply the brightness settings
            Debug.WriteLine($"Applying AC brightness {brightness}");

            // Bugfix: Set different two brightness values to force the brightness to change
            // (Maybe on surface devices. My device has this issue)
            if (brightness > 50) PowerConfigBrightnessController.Set(25);
            else PowerConfigBrightnessController.Set(75);

            // Wait for a while to let the brightness change (500ms is adequate)
            await Task.Delay(500);

            // Set the actual brightness
            PowerConfigBrightnessController.Set(brightness);
            s_wasOnBattery = false;
        }
    }

    /// <summary>
    /// Saves the current brightness settings to the settings file
    /// </summary>
    private static void SaveCurrentBrightnessSettings()
    {
        // Save the current brightness if are on battery
        if (IsOnBattery && s_wasOnBattery == true)
        {
            // If adaptive brightness is enabled, save the brightness as auto
            var isAdaptiveBrightnessEnabled = GetAdaptiveBrightnessStatus(false);
            if(isAdaptiveBrightnessEnabled == true)
            {
                Debug.WriteLine("Saving battery brightness: auto");
                IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyBattery, "auto");
                return;
            }

            // Otherwise, save the current brightness
            var currentBrightness = PowerConfigBrightnessController.Get(!IsOnBattery);
            Debug.WriteLine($"Saving battery brightness: {currentBrightness}");
            IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyBattery, currentBrightness.ToString());
        }
        // Save the current brightness if we are on AC
        else if(!IsOnBattery && s_wasOnBattery == false)
        {
            // If adaptive brightness is enabled, save the brightness as auto
            var isAdaptiveBrightnessEnabled = GetAdaptiveBrightnessStatus(true);
            if (isAdaptiveBrightnessEnabled == true)
            {
                Debug.WriteLine("Saving AC brightness: auto");
                IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyAc, "auto");
                return;
            }

            // Otherwise, save the current brightness
            var currentBrightness = PowerConfigBrightnessController.Get(!IsOnBattery);
            Debug.WriteLine($"Saving AC brightness: {currentBrightness}");
            IniFile.SetValue(SettingsPath, SettingsSection, SettingsKeyAc, currentBrightness.ToString());
        }
    }

    /// <summary>
    /// Indicates if the device is currently on battery
    /// </summary>
    private static bool IsOnBattery => PowerManager.PowerSupplyStatus == PowerSupplyStatus.NotPresent;

    /// <summary>
    /// Updates the text of the Startup Process Menu Flyout Item
    /// </summary>
    private void UpdateStartupProcessMenuFlyoutItemText()
    {
        // Get the menu flyout item
        var menuFlyoutItem = MfiStartupProcess;

        // Update the text of the menu flyout item based on the current startup process status
        menuFlyoutItem.Text = StartupProcessHelper.IsStartupProcess ? "Remove from Startup Process" : "Add to Startup Process";
    }

    // Menu Flyout Item Click Handlers
    private void OnOpenSettingsFileMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Process.Start("notepad.exe", SettingsPath);
    private void OnExitMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Environment.Exit(0);
    private void OnStartupProcessFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        // Toggle the startup process
        if (StartupProcessHelper.IsStartupProcess) StartupProcessHelper.RemoveStartupProcess();
        else StartupProcessHelper.SetupStartupProcess();

        // Update the text of the menu flyout item
        UpdateStartupProcessMenuFlyoutItemText();
    }

    /// <summary>
    /// This window is not supposed to be shown. Hide it when activated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnWindowActivated(object sender, WindowActivatedEventArgs args) => this.Hide();

    /// <summary>
    /// Gets the current status of the adaptive brightness feature
    /// </summary>
    /// <param name="isAC"></param>
    /// <returns>True if adaptive brightness is enabled, false if it is disabled, and null if it is not available</returns>
    private static bool? GetAdaptiveBrightnessStatus(bool isAC)
    {
        // Execute the powercfg command to get the current adaptive brightness settings.
        string powercfgOutput = ExecutePowerCfgQuery();

        // Determine the setting key based on the power supply status.
        string settingKey = isAC ? " AC " : " DC ";

        // Split the output into lines based on the environment's newline character.
        var lines = powercfgOutput.Split(Environment.NewLine, StringSplitOptions.None);

        // Iterate over each line to find one containing the "AC" or "DC" keyword.
        foreach (var line in lines)
        {
            if (line.Contains(settingKey))
            {
                // Check if the line contains "0x00000000" or "0x00000001" and return the corresponding boolean value.
                if (line.Contains("0x00000000"))
                {
                    return false;
                }
                else if (line.Contains("0x00000001"))
                {
                    return true;
                }
            }
        }

        // Return null if no matching setting is found.
        return null;
    }


    /// <summary>
    /// Executes the powercfg command to get the current adaptive brightness settings
    /// </summary>
    /// <returns>The output of the powercfg command</returns>
    private static string ExecutePowerCfgQuery()
    {
        // Create a new process to run the powercfg command.
        var process = new Process();
        process.StartInfo.FileName = "powercfg";
        process.StartInfo.Arguments = "/query SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Start the process and read the output.
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // Return the output of the powercfg command.
        return output;
    }

    private static void SetAdaptiveBrightness(bool enable)
    {
        // AC Power
        {
            var query = "/setacvalueindex SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
            var value = enable ? "1" : "0";
            var queryProcess = new Process();
            queryProcess.StartInfo.FileName = "powercfg";
            queryProcess.StartInfo.Arguments = $"{query} {value}";
            queryProcess.StartInfo.UseShellExecute = false;
            queryProcess.StartInfo.CreateNoWindow = true;
            queryProcess.Start();
            queryProcess.WaitForExit();
        }

        // DC Power
        {
            var query = "/setdcvalueindex SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
            var value = enable ? "1" : "0";
            var queryProcess = new Process();
            queryProcess.StartInfo.FileName = "powercfg";
            queryProcess.StartInfo.Arguments = $"{query} {value}";
            queryProcess.StartInfo.UseShellExecute = false;
            queryProcess.StartInfo.CreateNoWindow = true;
            queryProcess.Start();
            queryProcess.WaitForExit();
        }

        // Apply the settings
        var applyProcess = new Process();
        applyProcess.StartInfo.FileName = "powercfg";
        applyProcess.StartInfo.Arguments = "/S SCHEME_CURRENT";
        applyProcess.StartInfo.UseShellExecute = false;
        applyProcess.StartInfo.CreateNoWindow = true;
        applyProcess.Start();
        applyProcess.WaitForExit();
    }
}
