using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp
{
    public class MachineIdentifier
    {
        public static string GetUniqueMachineId()
        {
            try
            {
                string systemUuid = HardwareInfo.GetSystemUUID();
                string biosSerial = HardwareInfo.GetBIOSSerialNumber();
                string motherboardSerial = HardwareInfo.GetMotherboardSerialNumber();
                string cpuId = HardwareInfo.GetCPUId();

                // 组合硬件信息
                string combinedInfo = $"{systemUuid}-{biosSerial}-{motherboardSerial}-{cpuId}";

                Log.Information("generating unique machine ID: " + combinedInfo);

                // 计算 SHA256 哈希
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(combinedInfo);
                    byte[] hash = sha.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                Log.Information("Error generating unique machine ID: " + ex.Message);
                return "";
            }
        }
    }
}
