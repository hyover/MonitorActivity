using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;


namespace MonitorActivity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        /*
        * Rotation 0% = -142° / 100% = 53°
        * Mouvement = 142 + 53 = 295°
        * 295 / 100 = 2.95 par %
        */
        private SystemInfo si = new SystemInfo();
        DispatcherTimer Timer99 = new DispatcherTimer();
        DispatcherTimer TimerTemp = new DispatcherTimer();

        private CpuInfo cpu = new CpuInfo();
        private RamInfo ram = new RamInfo();
        private DiskInfo disk = new DiskInfo();
        private NetworkInfos net = new NetworkInfos();

        public MainWindow()
        {
            InitializeComponent();

            /*
             * Display OS INFO
             */

            _osName.Content = si.GetOsInfo("os");
            _osArch.Content = si.GetOsInfo("arch");
            _procName.Content = si.GetCpusInfo();
            _gpuName.Content = si.GetGpuInfo();
            _totalMemory.Content = si.GetTotalPhysicalMemory();

            /*
             * Display CPU Pourcentage + CPU TEMP
             */

            // Initialisation du timer pour le % et l'aiguille

            Timer99.Interval = TimeSpan.FromMilliseconds(1000); // Mettez à jour toutes les 1000 millisecondes
            Timer99.Tick += Timer99_Tick;
            Timer99.Start();


            // Display RAM INFO


            // Display DIK INFO
            List<string> driveNames = DiskInfo.GetDriveNames(); // Obtenir la liste des noms de disques
            UpdateDriveList(driveNames); // Mettre à jour la liste des disques

            // Display NETWORK INFO


            // END
            this.Closed += MainWindow_Closed;

        }



        /*
         * Minuteur + Get %
         */

        private async void Timer99_Tick(object sender, EventArgs e)
        {
            try
            {
                // Get CPU %
                var cpuUsageString = cpu.GetCurrentCpuUsage();
                _CPU.Content = cpuUsageString;

                if (double.TryParse(cpuUsageString.TrimEnd('%'), out double cpuUsage))
                {
                    UpdateAiguilleRotation(cpuUsage);
                }
                else
                {
                    Debug.WriteLine("Unable to parse CPU usage.");
                }

                // Update Temp
                await UpdateTemperatureAsync();

                // Update RAM
                UpdateUseMemory();
                await UpdateProgressBar();
                await UpdateFreeMemoryAsync();

                // Update Network
                await UpdateDownloadNetwork();
                await UpdateUploadNetwork();
            }
            catch (Exception ex)
            {
                // Here, you can log the exception or show a user-friendly error message.
                Debug.WriteLine($"Error during Timer99_Tick: {ex.Message}");
            }
        }





        /*
         * Update Aiguille
         */

        private void UpdateAiguilleRotation(double cpuUsage)
        {
            // Position 0% = -142° / Position 100% = 53°
            // Movement = 142 + 53 = 295°
            // 295 / 100 = 2.95° per %
            double angle = -142 + (cpuUsage * 2.95); // Calculate the angle based on the percentage

            // Look for the RotateTransform inside the TransformGroup
            RotateTransform rotateTransform = null;
            if (_aiguille.RenderTransform is TransformGroup transformGroup)
            {
                foreach (var transform in transformGroup.Children)
                {
                    if (transform is RotateTransform)
                    {
                        rotateTransform = (RotateTransform)transform;
                        break;
                    }
                }
            }

            if (rotateTransform != null)
            {
                // Update the needle rotation
                rotateTransform.Angle = angle;
            }
            else
            {
                // Handle or log the error
                Debug.WriteLine("RotateTransform not found.");
            }
        }


        public async Task UpdateTemperatureAsync()
        {
            try
            {
                string temperature = await cpu.GetCurrentCpuTemperatureAsync();
                _temp.Content = temperature;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
           
        }

        public void UpdateUseMemory()
        {
            float useMemory = si.GetTotalPhysicalFloatMemory() - ram.GetFreeFloatMemory();
            var result = float.Parse(useMemory.ToString("F1"));

            _useMemory.Content = $"Utilisée : {result} GB";
        }

        public async Task UpdateFreeMemoryAsync()
        {
            try
            {
                string freeMemory = await ram.GetFreeMemoryAsync();
                _freeMemory.Content = freeMemory;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task UpdateDownloadNetwork()
        {
            try
            {
                string downloadNetwork = await net.GetNetworkThroughputDownload();
                _download.Content = downloadNetwork;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
      

         public async Task UpdateUploadNetwork()
        {   
            try
            {
                string uploadNetwork = await net.GetNetworkThroughputUpload();
                _upload.Content = uploadNetwork;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task UpdateProgressBar()
        {
            try
            {
                // Get memory percentage
                float memoryPercentage = await ram.GetPurcentMemoryAsync();
                _progressBarMemory.Value = memoryPercentage; // Mise à jour de la ProgressBar
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void UpdateDriveList(List<string> driveNames)
        {
            _listDisk.Items.Clear(); // Effacer les éléments existants dans la liste

            foreach (string driveName in driveNames)
            {
                _listDisk.Items.Add(driveName); // Ajouter les noms des disques à la liste
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Timer99.Stop();

            // Libérez les ressources pour l'objet cpu.
            cpu.Dispose();

            // Ici, vous pouvez libérer d'autres ressources si nécessaire.
        }
    }
}
