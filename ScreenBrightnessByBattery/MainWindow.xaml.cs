using Microsoft.UI.Xaml;
using Microsoft.Windows.System.Power;
using ScreenBrightnessByBattery.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Power;
using WinUIEx;

namespace ScreenBrightnessByBattery;


public sealed partial class MainWindow
{
    // Constants
    private const string BrightnessSettingsBatteryBrightnessDefault = "70";
    private const string BrightnessSettingsAcBrightnessDefault = "100";
    private const string BrightnessSettingsSection = "Brightness";
    private const string BrightnessSettingsBatteryKey = "Battery";
    private const string BrightnessSettingsAcKey = "AC";

    /// <summary>
    /// Path to the settings file
    /// </summary>
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

    /// <summary>
    /// 1500ms is adequate interval.
    /// Change it if you want
    /// </summary>
    private static readonly System.Timers.Timer BrightnessTimer = new(1500);
    private static readonly System.Timers.Timer SleepTimer = new(1500);

    static MainWindow()
    {
        BrightnessTimer.Elapsed += async (s, e) =>
        {
            await ApplySettingsAsync(); // Application must be called first in order to prevent settings overwrite
            SaveCurrentBrightnessSettings();
        };
        BrightnessTimer.Start();
    }

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.IsShownInSwitchers = false;

        if(!File.Exists(SettingsPath))
        {
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, BrightnessSettingsBatteryBrightnessDefault);
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, BrightnessSettingsBatteryBrightnessDefault);
        }

        UpdateStartupProcessMenuFlyoutItemText();
    }

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
}
