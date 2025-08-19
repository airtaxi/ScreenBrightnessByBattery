using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ScreenBrightnessByBattery.Helpers;

/// <summary>
/// Helper class to control screen and system sleep mode
/// </summary>
public static partial class SleepModeHelper
{
    #region Windows API
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    // Replace the static readonly field with a static property to return a new Guid instance each time
    private static Guid GUID_DEVCLASS_MONITOR => new("4D36E96E-E325-11CE-BFC1-08002BE10318");

    private const int DIGCF_PRESENT = 0x00000002; // Device Manager default view (devices that are currently present)

    [StructLayout(LayoutKind.Sequential)]
    private struct SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable SYSLIB1054 // Using DllImport for compatibility;
    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevs(
        ref Guid ClassGuid,
        IntPtr Enumerator,
        IntPtr hwndParent,
        int Flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInfo(
        IntPtr DeviceInfoSet,
        uint MemberIndex,
        ref SP_DEVINFO_DATA DeviceInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079 // Remove unnecessary suppression

    // Constants for controlling screen saver and sleep mode
    [Flags]
    private enum ExecutionState : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001,
    }

    #endregion

    private static bool s_isPreventingActive = false;

    /// <summary>
    /// Gets the total number of display devices (including all physical monitors, even if powered off)
    /// </summary>
    /// <returns>Total number of display devices</returns>
    public static int GetTotalDisplayCount()
    {
        // Create a local variable to hold the GUID value
        Guid monitorClassGuid = GUID_DEVCLASS_MONITOR;
        IntPtr hDevInfo = SetupDiGetClassDevs(ref monitorClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT);
        if (hDevInfo == IntPtr.Zero || hDevInfo == new IntPtr(-1))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "SetupDiGetClassDevs failed.");

        try
        {
            int count = 0;
            var devInfo = new SP_DEVINFO_DATA { cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>() };

            for (uint i = 0; SetupDiEnumDeviceInfo(hDevInfo, i, ref devInfo); i++)
            {
                count++;
                devInfo.cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>(); // reset for next iteration
            }

            // End of enumeration -> ERROR_NO_MORE_ITEMS (259). Other errors should be surfaced.
            int err = Marshal.GetLastWin32Error();
            if (err != 0 && err != 259)
                throw new Win32Exception(err, "SetupDiEnumDeviceInfo returned an error.");

            return count;
        }
        finally
        {
            SetupDiDestroyDeviceInfoList(hDevInfo);
        }
    }

    /// <summary>
    /// Checks if multiple monitors are available (internal + external, regardless of power state)
    /// </summary>
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
