using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EasySaveConsole
{
    public static class CryptoSoftRunner
    {
        private static readonly object _cryptoLock = new object();

        private static string ResolveCryptoSoftPath()
        {
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CryptoSoft.exe");
            if (File.Exists(localPath))
                return localPath;
            DirectoryInfo? currentDir = new(AppDomain.CurrentDomain.BaseDirectory);
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
            return localPath;
        }

        public static double Encrypt(string source, string destination, string key = "MaCleSecrete123")
        {
            string exePath = ResolveCryptoSoftPath();

            if (!File.Exists(exePath))
                return -1;

            Stopwatch sw = new Stopwatch();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"\"{source}\" \"{destination}\" \"{key}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            try
            {
                lock (_cryptoLock)
                {
                    sw.Restart();
                    using Process? process = Process.Start(startInfo);
                    if (process == null)
                        return -2;

                    process.StandardError.ReadToEnd();
                    process.StandardOutput.ReadToEnd();
                    process.WaitForExit(60000);
                    sw.Stop();

                    if (process.ExitCode != 0)
                        return -Math.Abs(process.ExitCode);

                    return sw.Elapsed.TotalMilliseconds;
                }
            }
            catch
            {
                return -3;
            }
        }
    }
}
