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
    /// Handles the event when the sleep timer elapses, checking if the external monitor is connected and preventing sleep mode if necessary.
    /// </summary>
    private static void OnSleepTimerElapsed(object sender, ElapsedEventArgs e)
    {
        var isEnabled = IniFile.GetValue(SettingsPath, SleepSettingsSection, SleepSettingsPreventSleepWhenExternalMonitorConnectedKey, SleepSettingsPreventSleepWhenExternalMonitorConnectedOff) == SleepSettingsPreventSleepWhenExternalMonitorConnectedOn;

        var isActive = SleepModeHelper.IsPreventingActive();
        var isExternalMonitorConnected = SleepModeHelper.IsExternalMonitorConnected();

        if (isEnabled && isExternalMonitorConnected && !isActive) SleepModeHelper.PreventSleepMode(true);
        else if (!isEnabled && isActive) SleepModeHelper.PreventSleepMode(false);
    }
}
