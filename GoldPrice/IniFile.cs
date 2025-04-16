using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldPrice
{
    public static class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void WriteValue(string filePath, string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, filePath);
        }

        public static string ReadValue(string filePath, string section, string key, string defaultValue = "")
        {
            StringBuilder retVal = new StringBuilder(1024);
            GetPrivateProfileString(section, key, defaultValue, retVal, 1024, filePath);
            return retVal.ToString();
        }
    }

}
