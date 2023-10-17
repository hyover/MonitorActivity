using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class NetworkInfos
    {
        private long previousDownloadBytes = 0;
        private long previousUploadBytes = 0;

        public async Task<string> GetNetworkThroughputDownload()
        {
            return await Task.Run(() =>
            {
                try
                {
                    NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface networkInterface in networkInterfaces)
                    {
                        if (networkInterface.OperationalStatus == OperationalStatus.Up)
                        {
                            IPv4InterfaceStatistics statistics = networkInterface.GetIPv4Statistics();
                            long downloadBytes = statistics.BytesReceived;
                            double downloadSpeed = (downloadBytes - previousDownloadBytes) / 1024.0;  // Convertir en KBytes/s en tant que double
                            previousDownloadBytes = downloadBytes;

                            if (downloadSpeed >= 1024)
                            {
                                downloadSpeed /= 1024.0;  // Convertir en MBytes/s
                                return $"{downloadSpeed:F2} MBytes/s";
                            }
                            else
                            {
                                return $"{downloadSpeed:F2} KBytes/s";
                            }
                        }
                    }

                    return "N/A";
                }
                catch
                {
                    return "N/A";
                }
            });
        }

        public async Task<string> GetNetworkThroughputUpload()
        {
            return await Task.Run(() =>
            {
                try
                {
                    NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (NetworkInterface networkInterface in networkInterfaces)
                    {
                        if (networkInterface.OperationalStatus == OperationalStatus.Up)
                        {
                            IPv4InterfaceStatistics statistics = networkInterface.GetIPv4Statistics();
                            long uploadBytes = statistics.BytesSent;
                            double uploadSpeed = (uploadBytes - previousUploadBytes) / 1024.0;  // Convertir en KBytes/s en tant que double
                            previousUploadBytes = uploadBytes;

                            if (uploadSpeed >= 1024)
                            {
                                uploadSpeed /= 1024.0;  // Convertir en MBytes/s
                                return $"{uploadSpeed:F2} MBytes/s";
                            }
                            else
                            {
                                return $"{uploadSpeed:F2} KBytes/s";
                            }
                        }
                    }
                    return "N/A";
                }
                catch
                {
                    return "N/A";
                }
            });
        }

    }
}