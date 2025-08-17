using Microsoft.UI.Xaml;
using Microsoft.Windows.System.Power;
using ScreenBrightnessByBattery.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Power;
using WinUIEx;

namespace ScreenBrightnessByBattery;


public sealed partial class MainWindow
{
    // Constants
    private const string BooleanSettingsOn = "on";
    private const string BooleanSettingsOff = "off";

    private const string BrightnessSettingsSection = "Brightness";
    private const string SleepSettingsSection = "Sleep";

    private const string BrightnessSettingsBatteryKey = "Battery";
    private const string BrightnessSettingsBatteryDefault = "70";

    private const string BrightnessSettingsAcKey = "AC";
    private const string BrightnessSettingsAcBrightnessDefault = "100";

    private const string SleepSettingsPreventSleepWhenExternalMonitorConnectedKey = "Enabled";
    private const string SleepSettingsPreventSleepWhenExternalMonitorConnectedDefault = BooleanSettingsOff;

    private const string BrightnessSettingsEnabledKey = "Enabled";
    private const string BrightnessSettingsEnabledDefault = BooleanSettingsOn;

    /// <summary>
    /// Path to the settings file
    /// </summary>
    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

    /// <summary>
    /// 1500ms is adequate interval.
    /// Change it if you want
    /// </summary>
    private static readonly System.Timers.Timer BrightnessTimer = new(1500);

    /// <summary>
    /// Uses DispatcherTimer to ensure SetExecutionState is called on unique thread (UI thread).
    /// see: https://stackoverflow.com/questions/76111050/setthreadexecutionstate-with-just-es-continuous-dont-enable-sleep-on-windows-11s
    /// </summary>
    private static readonly DispatcherTimer SleepTimer = new() { Interval = TimeSpan.FromMilliseconds(1500) };

    static MainWindow()
    {
        BrightnessTimer.Elapsed += OnBrightnessTimerElapsed;
        BrightnessTimer.Start();

        SleepTimer.Tick += OnSleepTimerTick;
        SleepTimer.Start();
    }

    public MainWindow()
    {
        InitializeComponent();

        AppWindow.IsShownInSwitchers = false;
        AppWindow.SetIcon("Icon.ico");

        if(!File.Exists(SettingsPath))
        {
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsEnabledKey, BrightnessSettingsEnabledDefault);
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, BrightnessSettingsBatteryDefault);
            IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsBatteryKey, BrightnessSettingsBatteryDefault);
            IniFile.SetValue(SettingsPath, SleepSettingsSection, SleepSettingsPreventSleepWhenExternalMonitorConnectedKey, SleepSettingsPreventSleepWhenExternalMonitorConnectedDefault);
        }

        UpdateScreenBrightnessByBatteryMenuFlyoutItemText();
        UpdatePreventSleepSettingsMenuFlyoutItemText();
        UpdateStartupProcessMenuFlyoutItemText();
    }

    /// <summary>
    /// Updates the text of the Screen Brightness by Battery Menu Flyout Item
    /// </summary>
    private void UpdateScreenBrightnessByBatteryMenuFlyoutItemText()
    {
        // Get the menu flyout item
        var menuFlyoutItem = MfiScreenBrightnessByBattery;

        // Update the text of the menu flyout item based on the current screen brightness by battery status
        menuFlyoutItem.Text =
            IniFile.GetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsEnabledKey, BrightnessSettingsEnabledDefault) == BooleanSettingsOn
            ? "Screen Brightness by Battery: Enabled"
            : "Screen Brightness by Battery: Disabled";
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

    /// <summary>
    /// Updates the text of the Prevent Sleep Settings Menu Flyout Item
    /// </summary>
    private void UpdatePreventSleepSettingsMenuFlyoutItemText()
    {
        // Get the menu flyout item
        var menuFlyoutItem = MfiPreventSleepSettings;

        // Update the text of the menu flyout item based on the current prevent sleep settings status
        MfiPreventSleepSettings.Text =
            IniFile.GetValue(SettingsPath, SleepSettingsSection, SleepSettingsPreventSleepWhenExternalMonitorConnectedKey, SleepSettingsPreventSleepWhenExternalMonitorConnectedDefault) == BooleanSettingsOn
            ? "Prevent Sleep (External Monitor): Enabled"
            : "Prevent Sleep (External Monitor): Disabled";
    }

    // Menu Flyout Item Click Handlers
    private void OnOpenSettingsFileMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Process.Start("notepad.exe", SettingsPath);
    private void OnExitProcessMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Environment.Exit(0);

    private void OnStartupProcessMenuFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        // Toggle the startup process
        if (StartupProcessHelper.IsStartupProcess) StartupProcessHelper.RemoveStartupProcess();
        else StartupProcessHelper.SetupStartupProcess();

        // Update the text of the menu flyout item
        UpdateStartupProcessMenuFlyoutItemText();
    }

    private void OnPreventSleepSettingsMenuFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        var isEnabled = IniFile.GetValue(SettingsPath, SleepSettingsSection, SleepSettingsPreventSleepWhenExternalMonitorConnectedKey, SleepSettingsPreventSleepWhenExternalMonitorConnectedDefault) == BooleanSettingsOn;
        var newValue = isEnabled ? BooleanSettingsOff : BooleanSettingsOn;
        IniFile.SetValue(SettingsPath, SleepSettingsSection, SleepSettingsPreventSleepWhenExternalMonitorConnectedKey, newValue);

        UpdatePreventSleepSettingsMenuFlyoutItemText();
    }

    private void OnScreenBrightnessByBatteryMenuFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        var isEnabled = IniFile.GetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsEnabledKey, BrightnessSettingsEnabledDefault) == BooleanSettingsOn;
        var newValue = isEnabled ? BooleanSettingsOff : BooleanSettingsOn;
        IniFile.SetValue(SettingsPath, BrightnessSettingsSection, BrightnessSettingsEnabledKey, newValue);

        UpdateScreenBrightnessByBatteryMenuFlyoutItemText();
    }

    /// <summary>
    /// This window is not supposed to be shown. Hide it when activated
    /// </summary>
    private void OnWindowActivated(object sender, WindowActivatedEventArgs args) => this.Hide();
}
