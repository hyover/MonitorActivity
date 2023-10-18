using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;

namespace MonitorActivity
{
    public class SystemInfo
    {
        /*
         * Get Info OS
         */
        public string GetOsInfo(string param)
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in mos.Get())
            {
                switch (param)
                {
                    case "os":
                        if (obj.Properties["Caption"] != null)
                        {
                            return obj.Properties["Caption"].Value.ToString();
                        }
                        break;
                    case "arch":
                        if (obj.Properties["OSArchitecture"] != null)
                        {
                            var osArchValue = obj.Properties["OSArchitecture"].Value.ToString();
                            var osArchProcValue = new string(osArchValue.Where(char.IsDigit).ToArray());

                            var osArch = $"Système d’exploitation {osArchValue}, processeur x{osArchProcValue}";

                            return osArch;
                        }
                        break;
                    case "osv":
                        if (obj.Properties["CSDVersion"] != null)
                        {
                            return obj.Properties["CSDVersion"].Value.ToString();
                        }
                        break;
                }
            }
            return "";
        }


        /*
         * Get Info CPU
         */

        public string GetCpusInfo()
        {
            RegistryKey processorName = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0");

            if (processorName != null)
            {
                object processorNameValue = processorName.GetValue("ProcessorNameString");
                if (processorNameValue != null)
                {
                    return processorNameValue.ToString();
                }
            }

            return "";
        }

        public string GetGpuInfo()
        {
            string gpuInfo = "";
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            foreach (ManagementObject obj in mos.Get())
            {
                long adapterRAM = Convert.ToInt64(obj["AdapterRAM"]);
                double adapterRAMInGB = (double)adapterRAM / (1024 * 1024 * 1024); // Conversion en gigaoctets

                gpuInfo += obj["Name"] + " " + adapterRAMInGB.ToString("0.0") + "GB" + " | " + "Version: " + obj["DriverVersion"];
            }

            return gpuInfo;
        }

        public string GetTotalPhysicalMemory()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject obj in mos.Get())
            {
                ulong totalMemoryInBytes = (ulong)obj["TotalPhysicalMemory"];
                double totalMemoryInGB = totalMemoryInBytes / (1024 * 1024 * 1024); // Conversion en gigaoctets

                totalMemoryInGB.ToString("0.0");

                var total = $"Total : {totalMemoryInGB} GB";

                return total;
            }

            return "N/A";
        }

        public float GetTotalPhysicalFloatMemory()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject obj in mos.Get())
            {
                ulong totalMemoryInBytes = (ulong)obj["TotalPhysicalMemory"];
                double totalMemoryInGB = totalMemoryInBytes / (1024 * 1024 * 1024); // Conversion en gigaoctets

                return (float)totalMemoryInGB;
            }

            return 0;
        }
    }
}