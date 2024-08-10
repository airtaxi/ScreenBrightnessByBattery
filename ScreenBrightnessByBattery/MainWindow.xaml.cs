using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.System.Power;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Power;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenBrightnessByBattery;


public sealed partial class MainWindow : Window
{
    const uint MONITOR_DEFAULTTOPRIMARY = 1;
    private const string DefaultRawBatteryBrightness = "70";
    private const string DefaultRawAcBrightness = "100";

    private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");

    [LibraryImport("dxva2.dll", EntryPoint = "GetMonitorBrightness", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMonitorBrightness(IntPtr hMonitor, out int pdwMinimumBrightness, out int pdwCurrentBrightness, out int pdwMaximumBrightness);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr GetDesktopWindow();

    [LibraryImport("dxva2.dll", EntryPoint = "SetMonitorBrightness", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetMonitorBrightness(IntPtr hMonitor, int dwNewBrightness);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);


    public MainWindow()
    {
        InitializeComponent();

        AppWindow.IsShownInSwitchers = false;

        var battery = Battery.AggregateBattery;
        battery.ReportUpdated += OnBatteryReportUpdated;

        if(!File.Exists(SettingsPath))
        {
            var result = IniFile.SetValue(SettingsPath, "Brightness", "Battery", DefaultRawBatteryBrightness);
            result = IniFile.SetValue(SettingsPath, "Brightness", "AC", DefaultRawAcBrightness);
        }

        UpdateStartupProcessMenuFlyoutItemText();
        ApplySettings();
    }

    private void OnBatteryReportUpdated(Battery sender, object args) => ApplySettings();

    private static void ApplySettings()
    {
        var report = Battery.AggregateBattery.GetReport();
        var status = PowerManager.PowerSupplyStatus;

        if (status == PowerSupplyStatus.NotPresent)
        {
            var rawBrightness = IniFile.GetValue(SettingsPath, "Brightness", "Battery", DefaultRawBatteryBrightness);
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(DefaultRawBatteryBrightness);
            WindowsSettingsBrightnessController.Set(brightness);
        }
        else if (status == PowerSupplyStatus.Adequate || status == PowerSupplyStatus.Inadequate)
        {
            var rawBrightness = IniFile.GetValue(SettingsPath, "Brightness", "AC", DefaultRawAcBrightness);
            var success = int.TryParse(rawBrightness, out int brightness);
            if (!success) brightness = int.Parse(DefaultRawAcBrightness);
            WindowsSettingsBrightnessController.Set(brightness);
        }
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args) => this.Hide();

    private void OnOpenSettingsFileMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start("notepad.exe", SettingsPath);
    private void OnApplySettingsNowMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => ApplySettings();
    private void OnExitMenuFlyoutItemClicked(object sender, RoutedEventArgs e) => Environment.Exit(0);

    private void OnStartupProcessFlyoutItemClicked(object sender, RoutedEventArgs e)
    {
        if (StartupProcessHelper.IsStartupProcess) StartupProcessHelper.RemoveStartupProcess();
        else StartupProcessHelper.SetupStartupProcess();
        UpdateStartupProcessMenuFlyoutItemText();
    }

    private void UpdateStartupProcessMenuFlyoutItemText()
    {
        var menuFlyoutItem = MfiStartupProcess;
        menuFlyoutItem.Text = StartupProcessHelper.IsStartupProcess ? "Remove from Startup Process" : "Add to Startup Process";
    }
}
