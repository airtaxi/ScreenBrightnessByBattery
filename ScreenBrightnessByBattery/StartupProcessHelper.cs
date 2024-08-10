using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery;

public static class StartupProcessHelper
{
    private static readonly string AppName = "ScreenBrightnessByBattery";
    private static readonly string AppPath = Process.GetCurrentProcess().MainModule.FileName;

    public static bool IsStartupProcess
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            return key.GetValue(AppName) != null;
        }
    }

    public static void SetupStartupProcess()
    {
        using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (key.GetValue(AppName) != null) return;
        key.SetValue(AppName, AppPath);
    }

    public static void RemoveStartupProcess()
    {
        using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (key.GetValue(AppName) == null) return;
        key.DeleteValue(AppName);
    }
}
