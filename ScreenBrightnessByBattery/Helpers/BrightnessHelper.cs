using System;
using System.Management;

namespace ScreenBrightnessByBattery.Helpers;

public static class BrightnessHelper
{
    private const string WmiScope = @"root\WMI";

    /// <summary>
    /// Retrieves the current display brightness value via WMI.
    /// </summary>
    /// <returns>The current brightness value (0-100).</returns>
    /// <exception cref="Exception">Thrown when the brightness value cannot be retrieved.</exception>
    public static int Get()
    {
        using var searcher = new ManagementObjectSearcher(WmiScope, "SELECT CurrentBrightness FROM WmiMonitorBrightness");

        foreach (var instance in searcher.Get())
            return (byte)instance["CurrentBrightness"];

        throw new Exception("Failed to retrieve the current brightness value.");
    }

    /// <summary>
    /// Sets the display brightness value directly via WMI.
    /// </summary>
    /// <param name="value">The brightness value to set (0-100).</param>
    public static void Set(int value)
    {
        using var searcher = new ManagementObjectSearcher(WmiScope, "SELECT * FROM WmiMonitorBrightnessMethods");

        foreach (var instance in searcher.Get())
        {
            var managementObject = (ManagementObject)instance;
            var parameters = managementObject.GetMethodParameters("WmiSetBrightness");
            parameters["Timeout"] = (uint)1;
            parameters["Brightness"] = (byte)value;
            managementObject.InvokeMethod("WmiSetBrightness", parameters, null);
        }
    }
}
