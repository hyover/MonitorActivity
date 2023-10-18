using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class NetworkInfos
    {
        private Dictionary<string, long> previousBytes = new Dictionary<string, long>
        {
            { "Download", 0 },
            { "Upload", 0 }
        };

        private async Task<string> GetNetworkThroughput(Func<IPv4InterfaceStatistics, long> getBytesFunc, string type)
        {
            return await Task.Run(() =>
            {
                try
                {
                    NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                    var activeInterfaces = networkInterfaces.Where(ni => ni.OperationalStatus == OperationalStatus.Up);
                    foreach (NetworkInterface networkInterface in activeInterfaces)
                    {
                        IPv4InterfaceStatistics statistics = networkInterface.GetIPv4Statistics();
                        long bytes = getBytesFunc(statistics);
                        double speed = (bytes - previousBytes[type]) / 1024.0;
                        previousBytes[type] = bytes;
                        return FormatSpeed(speed);
                    }
                    return "N/A";
                }
                catch
                {
                    return "N/A"; // Possibilité d'ajouter une logique de journalisation ici
                }
            });
        }

        private string FormatSpeed(double speed)
        {
            if (speed >= 1024)
            {
                speed /= 1024.0;
                return $"{speed:F2} MBytes/s";
            }
            return $"{speed:F2} KBytes/s";
        }

        public Task<string> GetNetworkThroughputDownload()
        {
            return GetNetworkThroughput(stat => stat.BytesReceived, "Download");
        }

        public Task<string> GetNetworkThroughputUpload()
        {
            return GetNetworkThroughput(stat => stat.BytesSent, "Upload");
        }
    }
}
