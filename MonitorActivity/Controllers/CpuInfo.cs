using LibreHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class CpuInfo : IDisposable
    {
        private string lastValidTemperature = "N/A";
        private readonly PerformanceCounter cpuCounter;
        private readonly Computer computer;
        private bool disposed = false; // Pour détecter les appels redondants

        public CpuInfo()
        {
            // Créez le compteur de performance CPU dans le constructeur
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            // Initialisation de l'objet Computer
            computer = new Computer { IsCpuEnabled = true };
            computer.Open();
        }

        public string GetCurrentCpuUsage()
        {
            double cpuUsage = cpuCounter.NextValue();
            string formattedCpuUsage = cpuUsage.ToString("F2") + "%";
            return formattedCpuUsage;
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }

            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        public async Task<string> GetCurrentCpuTemperatureAsync()
        {
            return await Task.Run(() =>
            {
                string temperatureString = "N/A";
                UpdateVisitor updateVisitor = new UpdateVisitor();

                try
                {
                    computer.Accept(updateVisitor);

                    Debug.WriteLine(message: $"Total hardware components detected: {computer.Hardware}");

                    foreach (var hardware in computer.Hardware)
                    {
                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            Debug.WriteLine("Hardware is of type CPU.");

                            hardware.Update();
                            Debug.WriteLine($"Total sensors detected for CPU: {hardware.Sensors.Length}");

                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature)
                                {
                                    Debug.WriteLine($"Sensor name: {sensor.Name}, value: {sensor.Value}");

                                    if (sensor.Value.HasValue)
                                    {
                                        temperatureString = $"{sensor.Value.Value.ToString("F1")}°C";
                                        lastValidTemperature = temperatureString;
                                    }
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return lastValidTemperature;
            });
        }

        public void Dispose()
        {
            // Ne changez rien ici. Dispose(bool disposing) s'en occupe.
            Dispose(true);

            // Suppression du finalizer pour le garbage collector.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Libérez les ressources gérées ici, si nécessaire.
                }

                // Libérez les ressources non gérées ici.
                computer.Close();

                disposed = true;
            }
        }
    }
}
