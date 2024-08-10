using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenBrightnessByBattery
{
    public static partial class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static long SetValue(string path, string section, string key, string value) => WritePrivateProfileString(section, key, value, path);

        public static string GetValue(string path, string section, string key, string @default)
        {
            var builder = new StringBuilder(255);
            GetPrivateProfileString(section, key, @default, builder, 255, path);
            if (builder != null && builder.Length > 0) return builder.ToString();
            else return @default;
        }
    }

}
