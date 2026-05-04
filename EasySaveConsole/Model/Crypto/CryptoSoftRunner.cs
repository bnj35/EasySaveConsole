using System;
using System.Diagnostics;

public static class CryptoSoftRunner
{
    // Chemin absolu vers votre exécutable CryptoSoft
    private const string CryptoSoftPath = @"C:\Users\Omen\Documents\Projet CESI\A3\Projet 5\EasySaveConsole\CryptoSoft\bin\Debug\net10.0\CryptoSoft.exe";

    public static void Encrypt(string source, string destination, string key = "MaCleSecrete123")
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = CryptoSoftPath,
            Arguments = $"\"{source}\" \"{destination}\" \"{key}\"",
            UseShellExecute = false,
            CreateNoWindow = true, 
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (Process? process = Process.Start(startInfo))
        {
            if (process == null)
            {
                throw new InvalidOperationException(LanguageService.T("error.cryptosoft.failed"));
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string errorMessage = string.Format(LanguageService.T("error.cryptosoft.failed"), process.ExitCode);
                throw new Exception(errorMessage);
            }
        }
    }
}
