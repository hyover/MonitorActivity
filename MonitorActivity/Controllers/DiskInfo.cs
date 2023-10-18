using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace MonitorActivity
{
    class DiskInfo
    {
        public static List<string> GetDriveNames()
        {
            // Récupération de la liste des disques disponibles
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Extraction des noms des disques
            List<string> driveNames = new List<string>();
            foreach (DriveInfo drive in drives)
            {
                // Conversion des valeurs en gigaoctets
               
                double totalSizeGB = (double)drive.TotalSize / 1073741824; // Total Size en Go
                double totalFreeSpaceGB = (double)drive.TotalFreeSpace / 1073741824; // Total Free Space en Go

                // Ajout des informations du disque formatées en Go
                driveNames.Add($"{drive.Name} ({drive.DriveFormat}) {totalFreeSpaceGB:F2} Go libres / {totalSizeGB:F2} Go");
            }


            return driveNames;
        }
    }
}
