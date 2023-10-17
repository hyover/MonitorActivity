using OpenHardwareMonitor.Hardware;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonitorActivity
{
    class CpuInfo
    {
        private string lastValidTemperature = "N/A";
        PerformanceCounter cpuCounter;

        public CpuInfo()
        {
            // Créez le compteur de performance CPU dans le constructeur
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
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
                try
                {
                    hardware.Update();
                    foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
                }
                catch
                {

                }
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        public async Task<string> GetCurrentCpuTemperatureAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string temperatureString = "N/A";

                    UpdateVisitor updateVisitor = new UpdateVisitor();
                    Computer computer = new Computer();

                    // Tente d'ouvrir la connexion
                    computer.Open();
                   
                    // Active la surveillance du CPU
                    computer.CPUEnabled = true;
                    computer.Accept(updateVisitor);

                    foreach (var hardware in computer.Hardware)
                    {
                        if (hardware.HardwareType == HardwareType.CPU)
                        {
                            try
                            {
                                hardware.Update();
                            }
                            catch
                            {

                            }
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature)
                                {
                                    if (sensor.Value.HasValue) // Vérifiez si une valeur numérique est disponible
                                    {
                                        temperatureString = $"{sensor.Value}°C";
                                        lastValidTemperature = temperatureString; // Stockez la dernière valeur valide
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // Ferme la connexion
                    computer.Close();

                    return lastValidTemperature;
                }
                catch
                {
                    // Gérer l'exception ici (par exemple, journalisation ou affichage d'un message d'erreur)
                    return lastValidTemperature; // Si une exception se produit, retournez la dernière valeur valide
                }
            });
        }


    }
}