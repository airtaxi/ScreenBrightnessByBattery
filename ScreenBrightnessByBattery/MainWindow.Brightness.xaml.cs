using Microsoft.Windows.System.Power;
using ScreenBrightnessByBattery.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ScreenBrightnessByBattery;

public sealed partial class MainWindow
{
    /// <summary>
    /// Handles the event when the brightness timer elapses, applying settings and saving the current brightness
    /// configuration.
    /// </summary>
    private static async void OnBrightnessTimerElapsed(object sender, ElapsedEventArgs e)
    {
        await ApplySettingsAsync(); // Application must be called first in order to prevent settings overwrite
        SaveCurrentBrightnessSettings();
    }

    /// <summary>
    /// Indicates if the last time we checked the power supply status, we were on battery or not
    /// Null means we haven't checked yet
    /// </summary>
    private static bool? s_wasOnBattery;

    /// <summary>
    /// Indicates if the device is currently on battery
    /// </summary>
    private static bool IsOnBattery => PowerManager.PowerSupplyStatus == PowerSupplyStatus.NotPresent;

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
            var rawBrightness = IniFile.GetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, BrightnessSettingsBatteryBrightnessDefault);

            // Apply the brightness settings if raw brightness is auto
            if (rawBrightness == "auto")
            {
                Debug.WriteLine("Applying battery brightness: auto");
                AdaptiveBrightnessHelper.SetAdaptiveBrightness(true);
                s_wasOnBattery = true;
                return;
            }
            // Turn off adaptive brightness if it is enabled
            else
            {
                var isAdaptiveBrightnessEnabled = AdaptiveBrightnessHelper.GetAdaptiveBrightnessStatus(false);
                if (isAdaptiveBrightnessEnabled == true) AdaptiveBrightnessHelper.SetAdaptiveBrightness(false);
            }

            // Otherwise, apply the brightness settings
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(BrightnessSettingsBatteryBrightnessDefault);

            // Apply the brightness settings
            Debug.WriteLine($"Applying battery brightness {brightness}");

            // Bugfix: Set different two brightness values to force the brightness to change
            // (Maybe on surface devices. My device has this issue)
            if (brightness > 50) PowerConfigBrightnessHelper.Set(25);
            else PowerConfigBrightnessHelper.Set(75);

            // Wait for a while to let the brightness change (500ms is adequate)
            await Task.Delay(500);

            // Set the actual brightness
            PowerConfigBrightnessHelper.Set(brightness);
            s_wasOnBattery = true;
        }
        else
        {
            // If we were on AC last time, do nothing
            if (s_wasOnBattery == false) return;

            // Get the brightness settings from the settings file
            var rawBrightness = IniFile.GetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsAcKey, BrightnessSettingsAcBrightnessDefault);
            // Apply the brightness settings if raw brightness is auto
            if (rawBrightness == "auto")
            {
                Debug.WriteLine("Applying AC brightness: auto");
                AdaptiveBrightnessHelper.SetAdaptiveBrightness(true);
                s_wasOnBattery = false;
                return;
            }
            // Turn off adaptive brightness if it is enabled
            else
            {
                var isAdaptiveBrightnessEnabled = AdaptiveBrightnessHelper.GetAdaptiveBrightnessStatus(true);
                if (isAdaptiveBrightnessEnabled == true) AdaptiveBrightnessHelper.SetAdaptiveBrightness(false);
            }

            // Otherwise, apply the brightness settings
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(BrightnessSettingsAcBrightnessDefault);

            // Apply the brightness settings
            Debug.WriteLine($"Applying AC brightness {brightness}");

            // Bugfix: Set different two brightness values to force the brightness to change
            // (Maybe on surface devices. My device has this issue)
            if (brightness > 50) PowerConfigBrightnessHelper.Set(25);
            else PowerConfigBrightnessHelper.Set(75);

            // Wait for a while to let the brightness change (500ms is adequate)
            await Task.Delay(500);

            // Set the actual brightness
            PowerConfigBrightnessHelper.Set(brightness);
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
            var isAdaptiveBrightnessEnabled = AdaptiveBrightnessHelper.GetAdaptiveBrightnessStatus(false);
            if (isAdaptiveBrightnessEnabled == true)
            {
                Debug.WriteLine("Saving battery brightness: auto");
                IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, "auto");
                return;
            }

            // Otherwise, save the current brightness
            var currentBrightness = PowerConfigBrightnessHelper.Get(!IsOnBattery);
            Debug.WriteLine($"Saving battery brightness: {currentBrightness}");
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, currentBrightness.ToString());
        }
        // Save the current brightness if we are on AC
        else if (!IsOnBattery && s_wasOnBattery == false)
        {
            // If adaptive brightness is enabled, save the brightness as auto
            var isAdaptiveBrightnessEnabled = AdaptiveBrightnessHelper.GetAdaptiveBrightnessStatus(true);
            if (isAdaptiveBrightnessEnabled == true)
            {
                Debug.WriteLine("Saving AC brightness: auto");
                IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsAcKey, "auto");
                return;
            }

            // Otherwise, save the current brightness
            var currentBrightness = PowerConfigBrightnessHelper.Get(!IsOnBattery);
            Debug.WriteLine($"Saving AC brightness: {currentBrightness}");
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsAcKey, currentBrightness.ToString());
        }
    }
}
