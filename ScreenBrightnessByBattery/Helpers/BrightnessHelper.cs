using System;
using WmiLight;

namespace ScreenBrightnessByBattery.Helpers;

public static class BrightnessHelper
{
    private const string WmiScope = @"\\.\root\wmi";

    /// <summary>
    /// Retrieves the current display brightness value via WMI.
    /// </summary>
    /// <returns>The current brightness value (0-100).</returns>
    /// <exception cref="Exception">Thrown when the brightness value cannot be retrieved.</exception>
    public static int Get()
    {
        using var connection = new WmiConnection(WmiScope);

        foreach (var monitor in connection.CreateQuery("SELECT CurrentBrightness FROM WmiMonitorBrightness"))
            return monitor.GetPropertyValue<byte>("CurrentBrightness");

        throw new Exception("Failed to retrieve the current brightness value.");
    }

    /// <summary>
    /// Sets the display brightness value directly via WMI.
    /// </summary>
    /// <param name="value">The brightness value to set (0-100).</param>
    public static void Set(int value)
    {
        using var connection = new WmiConnection(WmiScope);

        foreach (var monitor in connection.CreateQuery("SELECT * FROM WmiMonitorBrightnessMethods"))
        {
            using var method = monitor.GetMethod("WmiSetBrightness");
            using var parameters = method.CreateInParameters();
            parameters.SetPropertyValue("Timeout", 1);
            parameters.SetPropertyValue("Brightness", (byte)value);
            monitor.ExecuteMethod<uint>(method, parameters, out _);
        }
    }
}
