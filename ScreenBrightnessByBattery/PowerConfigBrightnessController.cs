using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery;

public static partial class PowerConfigBrightnessController
{
    /// <summary>
    /// The GUID of the subgroup that contains the brightness setting.
    /// </summary>
    private const string SubgroupGuid = "7516b95f-f776-4464-8c53-06167f40cc99";

    /// <summary>
    /// The GUID of the setting that controls the brightness value.
    /// </summary>
    private const string SettingGuid = "aded5e82-b909-4619-9949-f5d71dac0bcb";

    /// <summary>
    /// Retrieves the current brightness value for the specified power supply status.
    /// </summary>
    /// <param name="isAC">Indicates if the power supply status is AC.</param>
    /// <returns>The current brightness value.</returns>
    /// <exception cref="Exception">thrown when the brightness value cannot be retrieved.</exception>
    public static int Get(bool isAC)
    {
        // Determine the setting key based on the power supply status.
        string settingKey = isAC ? " AC " : " DC ";

        // Create a new process to run the powercfg command.
        var process = new Process();
        process.StartInfo.FileName = "powercfg";
        process.StartInfo.Arguments = $"/q SCHEME_CURRENT {SubgroupGuid} {SettingGuid}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Start the process and read the output.
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // Split the output into lines based on the environment's newline character.
        var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Iterate over each line to find one containing the "AC" or "DC" keyword.
        foreach (var line in lines)
        {
            if (line.Contains(settingKey))
            {
                var regex = BrightnessHexRegex();
                var match = regex.Match(line);

                return match.Success ? Convert.ToInt32(match.Value.Replace("0x", string.Empty), 16) : throw new Exception("Failed to retrieve the current brightness value.");
            }
        }

        // If the brightness value cannot be retrieved, throw an exception.
        throw new Exception("Failed to retrieve the current brightness value.");
    }

    /// <summary>
    /// Sets the brightness value for both power supply statuses.
    /// </summary>
    /// <param name="value"></param>
    public static void Set(int value)
    {
        Set(value, true); // AC Power
        Set(value, false); // DC Power
    }

    /// <summary>
    /// Sets the brightness value for the specified power supply status.
    /// </summary>
    /// <param name="value">The brightness value to set.</param>
    /// <param name="isAC">Indicates if the power supply status is AC.</param>
    public static void Set(int value, bool isAC)
    {
        // Set the brightness value for the specified power supply status.
        var process = new Process();
        process.StartInfo.FileName = "powercfg";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        if (isAC) process.StartInfo.Arguments = $"-SETACVALUEINDEX SCHEME_CURRENT {SubgroupGuid} {SettingGuid} {value}";
        else process.StartInfo.Arguments = $"-SETDCVALUEINDEX SCHEME_CURRENT {SubgroupGuid} {SettingGuid} {value}";

        process.Start();
        process.WaitForExit();

        // Apply the settings immediately
        var applyProcess = new Process();
        applyProcess.StartInfo.FileName = "powercfg";
        applyProcess.StartInfo.Arguments = "/S SCHEME_CURRENT";
        applyProcess.StartInfo.UseShellExecute = false;
        applyProcess.StartInfo.CreateNoWindow = true;
        applyProcess.Start();
        applyProcess.WaitForExit();
    }

    /// <summary>
    /// Source Generator Regex Method 
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex(@"0x[0-9a-fA-F]+")]
    private static partial Regex BrightnessHexRegex();
}
