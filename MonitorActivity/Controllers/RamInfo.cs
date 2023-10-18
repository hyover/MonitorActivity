using System.Diagnostics;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class RamInfo
    {
        private readonly PerformanceCounter _memoryAvailableCounter;
        private readonly PerformanceCounter _memoryPurcentCounter;

        public RamInfo()
        {
            // Initialisation des compteurs de performance pour la mémoire
            _memoryAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", null); // FREE
            _memoryPurcentCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", null); // % use
        }

        // Cette méthode renvoie la mémoire libre sous forme de chaîne.
        public async Task<string> GetFormattedFreeMemoryAsync()
        {
            return await Task.Run(() =>
            {
                float freeMemoryInGigabytes = GetFreeMemoryInGigabytes();
                return $"Libre : {freeMemoryInGigabytes:F1} GB";
            });
        }

        // Cette méthode renvoie la mémoire libre sous forme de float.
        public float GetFreeMemoryInGigabytes()
        {
            float freeMemoryInMegaBytes = _memoryAvailableCounter.NextValue();
            return freeMemoryInMegaBytes / 1024; // 1 Go = 1024 Mo
        }

        // Cette méthode renvoie le pourcentage d'utilisation de la mémoire sous forme de float.
        public async Task<double> GetMemoryUsagePercentageAsync()
        {
            return await Task.Run(() => _memoryPurcentCounter.NextValue());
        }
    }
}
