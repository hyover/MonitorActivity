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
       

        }



        /*
         * Minuteur + Get %
         */

        private async void Timer99_Tick(object sender, EventArgs e)
        {
            // Get CPU %
            _CPU.Content = cpu.GetCurrentCpuUsage();
            // Param rotation de l'aiguille en fonction du pourcentage du CPU
            double cpuUsage = double.Parse(_CPU.Content.ToString().TrimEnd('%')); // Convertir la chaîne en double
            // Mettre à l'affichage l'aiguille
            UpdateAiguilleRotation(cpuUsage);

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


        /*
         * Update Aiguille
         */

        private void UpdateAiguilleRotation(double cpuUsage)
        {
            // Position 0% = -142° / Position 100% = 53°
            // Mouvement = 142 + 53 = 295°
            // 295 / 100 = 2.95° par %

            double angle = -142 + (cpuUsage * 2.95); // Calcul de l'angle en fonction du pourcentage

            // Recherchez le RotateTransform à l'intérieur du TransformGroup
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
                // Mettre à jour la rotation de l'aiguille
                rotateTransform.Angle = angle;
            }
        }

        public async Task UpdateTemperatureAsync()
        {
            string temperature = await cpu.GetCurrentCpuTemperatureAsync();
            _temp.Content = temperature;
        }

        public void UpdateUseMemory()
        {
            float useMemory = si.GetTotalPhysicalFloatMemory() - ram.GetFreeFloatMemory();
            var result = float.Parse(useMemory.ToString("F1"));

            _useMemory.Content = $"Utilisée : {result} GB";
        }

        public async Task UpdateFreeMemoryAsync()
        {
            string freeMemory = await ram.GetFreeMemoryAsync();
            _freeMemory.Content = freeMemory;
        }

        public async Task UpdateDownloadNetwork()
        {
            string downloadNetwork = await net.GetNetworkThroughputDownload();
            _download.Content = downloadNetwork;
        }
      

         public async Task UpdateUploadNetwork()
        {
            string uploadNetwork = await net.GetNetworkThroughputUpload();
            _upload.Content = uploadNetwork;
        }

        public async Task UpdateProgressBar()
        {
            // Get memory percentage
            float memoryPercentage = await ram.GetPurcentMemoryAsync();
            _progressBarMemory.Value = memoryPercentage; // Mise à jour de la ProgressBar
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

    }
}
