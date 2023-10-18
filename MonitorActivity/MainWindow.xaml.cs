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
    public partial class MainWindow : Window
    {
        private const double ANGLE_PER_PERCENTAGE = 2.95;
        private const double INITIAL_ANGLE = -142;
        private const int TIMER_INTERVAL_MILLISECONDS = 1000;

        private readonly SystemInfo _systemInfo = new SystemInfo();
        private readonly DispatcherTimer _updateTimer = new DispatcherTimer();
        private readonly CpuInfo _cpuInfo = new CpuInfo();
        private readonly RamInfo _ramInfo = new RamInfo();
        private readonly DiskInfo _diskInfo = new DiskInfo();
        private readonly NetworkInfos _networkInfo = new NetworkInfos();

        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemInfo();
            InitializeTimers();
            this.Closed += OnMainWindowClosed;
        }

        private void InitializeSystemInfo()
        {
            _osName.Content = _systemInfo.GetOsInfo("os");
            _osArch.Content = _systemInfo.GetOsInfo("arch");
            _procName.Content = _systemInfo.GetCpusInfo();
            _gpuName.Content = _systemInfo.GetGpuInfo();
            _totalMemory.Content = _systemInfo.GetTotalPhysicalMemory();
            UpdateDriveList(DiskInfo.GetDriveNames());
        }

        private void InitializeTimers()
        {
            _updateTimer.Interval = TimeSpan.FromMilliseconds(TIMER_INTERVAL_MILLISECONDS);
            _updateTimer.Tick += UpdateSystemStats;
            _updateTimer.Start();
        }

        private async void UpdateSystemStats(object sender, EventArgs e)
        {
            try
            {
                await RefreshSystemStatsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during UpdateSystemStats: {ex.Message}");
            }
        }

        private async Task RefreshSystemStatsAsync()
        {
            UpdateCpuStats();
            await UpdateTemperatureAsync();
            UpdateMemoryStats();
            await UpdateNetworkStatsAsync();
        }

        private void UpdateCpuStats()
        {
            var cpuUsageString = _cpuInfo.GetCurrentCpuUsage();
            _CPU.Content = cpuUsageString;
            Debug.WriteLine(cpuUsageString);


            if (double.TryParse(cpuUsageString.TrimEnd('%'), out double cpuUsage))
            {
                Debug.WriteLine(cpuUsage);

                UpdateNeedleRotation(cpuUsage);
            }
            else
            {
                Debug.WriteLine("Unable to parse CPU usage.");
            }
        }

        private void UpdateNeedleRotation(double cpuUsage)
        {
            double angle = INITIAL_ANGLE + (cpuUsage * ANGLE_PER_PERCENTAGE);

            // Obtenez le TransformGroup de l'image
            var transformGroup = _aiguille.RenderTransform as TransformGroup;

            if (transformGroup != null)
            {
                // Trouvez le RotateTransform dans le groupe
                foreach (var transform in transformGroup.Children)
                {
                    if (transform is RotateTransform rotation)
                    {
                        rotation.Angle = angle;
                        break;
                    }
                }
            }
            else
            {
                Debug.WriteLine("TransformGroup not found.");
            }
        }


        private async Task UpdateTemperatureAsync()
        {
            _temp.Content = await _cpuInfo.GetCurrentCpuTemperatureAsync();
        }

        private async Task UpdateMemoryStats()
        {
            UpdateUsedMemory();
            _progressBarMemory.Value = await _ramInfo.GetMemoryUsagePercentageAsync();
        }

        private void UpdateUsedMemory()
        {
            float usedMemory = _systemInfo.GetTotalPhysicalFloatMemory() - _ramInfo.GetFreeMemoryInGigabytes();
            _useMemory.Content = $"Utilisée: {usedMemory:F1} GB";
        }

        private async Task UpdateNetworkStatsAsync()
        {
            _download.Content = await _networkInfo.GetNetworkThroughputDownload();
            _upload.Content = await _networkInfo.GetNetworkThroughputUpload();
        }

        private void UpdateDriveList(IEnumerable<string> driveNames)
        {
            _listDisk.Items.Clear();
            foreach (string driveName in driveNames)
            {
                _listDisk.Items.Add(driveName);
            }
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            _updateTimer.Stop();
            _cpuInfo.Dispose();
        }
    }
}

