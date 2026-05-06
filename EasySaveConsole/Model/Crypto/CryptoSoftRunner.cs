using System;
using System.Diagnostics;
using System.IO;

namespace EasySaveConsole
{
    public static class CryptoSoftRunner
    {
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
            File.AppendAllText("crypto_debug.log", $"[{DateTime.Now:HH:mm:ss}] exe={exePath} exists={File.Exists(exePath)}\n  src={source}\n  dst={destination}\n");

            if (!File.Exists(exePath))
            {
                File.AppendAllText("crypto_debug.log", "  => ABORT: exe not found\n");
                return -1;
            }

            Stopwatch sw = Stopwatch.StartNew();

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
                using Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    File.AppendAllText("crypto_debug.log", "  => ABORT: Process.Start returned null\n");
                    return -2;
                }

                string stderr = process.StandardError.ReadToEnd();
                string stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit(60000);
                sw.Stop();

                File.AppendAllText("crypto_debug.log", $"  => exitCode={process.ExitCode} stdout={stdout} stderr={stderr}\n");

                if (process.ExitCode != 0)
                    return -Math.Abs(process.ExitCode);

                return sw.Elapsed.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                File.AppendAllText("crypto_debug.log", $"  => EXCEPTION: {ex.Message}\n");
                return -3;
            }
        }
    }
}
