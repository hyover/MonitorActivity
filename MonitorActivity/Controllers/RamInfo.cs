using System.Diagnostics;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class RamInfo
    {
        private PerformanceCounter _memoryAvailableCounter;
        private PerformanceCounter _memoryPurcentCounter;

        public RamInfo()
        {
            // Initialisez les compteurs de performance pour la mémoire
            _memoryAvailableCounter = new PerformanceCounter("Memory", "Available MBytes", null); // FREE
            _memoryPurcentCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", null); // % use
        }
     

        public async Task<string> GetFreeMemoryAsync()
        {
            return await Task.Run(() =>
            {
                float freeMemoryInMegaBytes = _memoryAvailableCounter.NextValue();
                float freeMemoryInGigabytes = freeMemoryInMegaBytes / 1024; // 1 Go = 1024 Mo

                // Utilisez la méthode ToString avec le format "F1" pour un chiffre après la virgule
                float freeMemory = float.Parse(freeMemoryInGigabytes.ToString("F1"));
                var result = freeMemory.ToString();
                return $"Libre : {result} GB";
            });
        }

        public float GetFreeFloatMemory()
        {
           
            float freeMemoryInMegaBytes = _memoryAvailableCounter.NextValue();
            float freeMemoryInGigabytes = freeMemoryInMegaBytes / 1024; // 1 Go = 1024 Mo

            // Utilisez la méthode ToString avec le format "F1" pour un chiffre après la virgule

            return float.Parse(freeMemoryInGigabytes.ToString("F1"));
            
        }




        public async Task<float> GetPurcentMemoryAsync()
        {
            return await Task.Run(() =>
            {
                return _memoryPurcentCounter.NextValue();
            });
        }


    }
}
