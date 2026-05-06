using System;
using System.Diagnostics;
using System.IO;

namespace EasySaveConsole
{
    public static class CryptoSoftRunner
    {
        private static string CryptoSoftPath
        {
            get
            {
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
                if (File.Exists(localPath))
                    return localPath;
                DirectoryInfo? currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (currentDir != null)
                {
                    string binDir = Path.Combine(currentDir.FullName, "CryptoSoft", "bin");
                    if (Directory.Exists(binDir))
                    {
                        string[] files = Directory.GetFiles(binDir, "CryptoSoft.exe", SearchOption.AllDirectories);
                        if (files.Length > 0) return files[0];
                    }
                    currentDir = currentDir.Parent;
                }

                return localPath; // Retourne le chemin par défaut si introuvable
            }
        }

        public static double Encrypt(string source, string destination, string key = "MaCleSecrete123")
        {
            if (!File.Exists(CryptoSoftPath))
            {
                return -1; 
            }

            Stopwatch sw = Stopwatch.StartNew();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = CryptoSoftPath,
                Arguments = $"\"{source}\" \"{destination}\" \"{key}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            

            try
            {
                using (Process? process = Process.Start(startInfo))
                {
                    if (process == null) return -2; 

                    process.WaitForExit(60000); 
                    sw.Stop();

                    if (process.ExitCode != 0)
                    {
                        return -Math.Abs(process.ExitCode); 
                    }
                }
                return sw.Elapsed.TotalMilliseconds;
            }
            catch
            {
                return -3; 
            }
        }
    }
}
