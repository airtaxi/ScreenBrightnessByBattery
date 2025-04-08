using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery.Helpers;

public class AdaptiveBrightnessHelper
{
    /// <summary>
    /// Gets the current status of the adaptive brightness feature
    /// </summary>
    /// <param name="isAC"></param>
    /// <returns>True if adaptive brightness is enabled, false if it is disabled, and null if it is not available</returns>
    public static bool? GetAdaptiveBrightnessStatus(bool isAC)
    {
        // Execute the powercfg command to get the current adaptive brightness settings.
        string powercfgOutput = ExecutePowerCfgQuery();

        // Determine the setting key based on the power supply status.
        string settingKey = isAC ? " AC " : " DC ";

        // Split the output into lines based on the environment's newline character.
        var lines = powercfgOutput.Split(Environment.NewLine, StringSplitOptions.None);

        // Iterate over each line to find one containing the "AC" or "DC" keyword.
        foreach (var line in lines)
        {
            if (line.Contains(settingKey))
            {
                // Check if the line contains "0x00000000" or "0x00000001" and return the corresponding boolean value.
                if (line.Contains("0x00000000"))
                {
                    return false;
                }
                else if (line.Contains("0x00000001"))
                {
                    return true;
                }
            }
        }

        // Return null if no matching setting is found.
        return null;
    }


    /// <summary>
    /// Executes the powercfg command to get the current adaptive brightness settings
    /// </summary>
    /// <returns>The output of the powercfg command</returns>
    private static string ExecutePowerCfgQuery()
    {
        // Create a new process to run the powercfg command.
        var process = new Process();
        process.StartInfo.FileName = "powercfg";
        process.StartInfo.Arguments = "/query SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        // Start the process and read the output.
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        // Return the output of the powercfg command.
        return output;
    }

    public static void SetAdaptiveBrightness(bool enable)
    {
        // AC Power
        {
            var query = "/setacvalueindex SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
            var value = enable ? "1" : "0";
            var queryProcess = new Process();
            queryProcess.StartInfo.FileName = "powercfg";
            queryProcess.StartInfo.Arguments = $"{query} {value}";
            queryProcess.StartInfo.UseShellExecute = false;
            queryProcess.StartInfo.CreateNoWindow = true;
            queryProcess.Start();
            queryProcess.WaitForExit();
        }

        // DC Power
        {
            var query = "/setdcvalueindex SCHEME_CURRENT SUB_VIDEO ADAPTBRIGHT";
            var value = enable ? "1" : "0";
            var queryProcess = new Process();
            queryProcess.StartInfo.FileName = "powercfg";
            queryProcess.StartInfo.Arguments = $"{query} {value}";
            queryProcess.StartInfo.UseShellExecute = false;
            queryProcess.StartInfo.CreateNoWindow = true;
            queryProcess.Start();
            queryProcess.WaitForExit();
        }

        // Apply the settings
        var applyProcess = new Process();
        applyProcess.StartInfo.FileName = "powercfg";
        applyProcess.StartInfo.Arguments = "/S SCHEME_CURRENT";
        applyProcess.StartInfo.UseShellExecute = false;
        applyProcess.StartInfo.CreateNoWindow = true;
        applyProcess.Start();
        applyProcess.WaitForExit();
    }
}
