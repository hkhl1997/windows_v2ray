using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp
{
    public class HardwareInfo
    {
        public static string GetWMIProperty(string wmiClass, string propertyName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj[propertyName]?.ToString().Trim() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                Log.Information($"Error retrieving {propertyName} from {wmiClass}: {ex.Message}");
            }
            return "";
        }

        public static string GetSystemUUID()
        {
            return GetWMIProperty("Win32_ComputerSystemProduct", "UUID");
        }

        public static string GetBIOSSerialNumber()
        {
            return GetWMIProperty("Win32_BIOS", "SerialNumber");
        }

        public static string GetMotherboardSerialNumber()
        {
            return GetWMIProperty("Win32_BaseBoard", "SerialNumber");
        }

        public static string GetCPUId()
        {
            return GetWMIProperty("Win32_Processor", "ProcessorId");
        }
    }
}
