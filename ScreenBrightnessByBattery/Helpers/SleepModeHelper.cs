using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery.Helpers;

/// <summary>
/// Helper class to control screen and system sleep mode
/// </summary>
public static partial class SleepModeHelper
{
    #region Windows API
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)]
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
    private static extern bool EnumDisplayDevices(
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore IDE0079 // Remove unnecessary suppression
        string lpDevice,
        uint iDevNum,
        ref DISPLAY_DEVICE lpDisplayDevice,
        uint dwFlags);

    // Constants for controlling screen saver and sleep mode
    [Flags]
    private enum ExecutionState : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001,
    }

    // Constants for EnumDisplayDevices
    private const uint DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAY_DEVICE
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }
    #endregion

    private static bool s_isPreventingActive = false;

    /// <summary>
    /// Gets the total number of display devices (including all physical monitors, even if powered off)
    /// </summary>
    /// <returns>Total number of display devices</returns>
    public static int GetTotalDisplayCount()
    {
        var count = 0;
        var device = new DISPLAY_DEVICE();
        device.cb = (uint)Marshal.SizeOf(device);

        uint deviceIndex = 0;
        while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
        {
            // Count all display devices except virtual mirroring drivers
            if ((device.StateFlags & DISPLAY_DEVICE_MIRRORING_DRIVER) == 0) count++;

            deviceIndex++;
        }

        return count;
    }

    /// <summary>
    /// Checks if multiple monitors are available (internal + external, regardless of power state)
    /// </summary>
    /// <returns>True if more than one display is available</returns>
    public static bool HasMultipleDisplays() => GetTotalDisplayCount() > 1;

    /// <summary>
    /// Prevents or allows the system to enter sleep mode
    /// </summary>
    /// <param name="prevent">True to prevent sleep mode, false to allow it</param>
    /// <returns>True if the operation was successful</returns>
    public static bool PreventSleepMode(bool prevent)
    {
        try
        {
            ExecutionState result;

            if (prevent)
            {
                // Disable screen saver and sleep mode
                result = SetThreadExecutionState(
                    ExecutionState.ES_CONTINUOUS |
                    ExecutionState.ES_DISPLAY_REQUIRED |
                    ExecutionState.ES_SYSTEM_REQUIRED);

                // Check if the function call was successful (non-zero return value)
                s_isPreventingActive = result != 0;
            }
            else
            {
                // Restore default system settings
                result = SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
                s_isPreventingActive = false;
            }

            // Return true if the operation was successful
            return result != 0;
        }
        catch (Exception)
        {
            s_isPreventingActive = false;
            return false;
        }
    }

    /// <summary>
    /// Gets the current sleep prevention status
    /// </summary>
    /// <returns>True if sleep prevention is active</returns>
    public static bool IsPreventingActive()
    {
        return s_isPreventingActive;
    }
}
