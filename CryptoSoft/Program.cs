using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CryptoSoft
{
    class Program
    {
        static int Main(string[] args)
        {
            Mutex mutex = new Mutex(true, "Global\\CryptoSoftMutex", out bool createdNew);
            if (!createdNew)
            {
                return -5;
            }
            
            try
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: CryptoSoft.exe <source_file> <destination_file> <key>");
                    return -1; 
                }

                string sourcePath = args[0];
                string destPath = args[1];
                string key = args[2];

                if (!File.Exists(sourcePath))
                {
                    return -2; 
                }

                try
                {
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    int keyLength = keyBytes.Length;
                    long keyIndex = 0;

                    using (FileStream fsIn = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                    using (FileStream fsOut = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            for (int i = 0; i < bytesRead; i++)
                            {
                                buffer[i] = (byte)(buffer[i] ^ keyBytes[keyIndex % keyLength]);
                                keyIndex++;
                            }
                            fsOut.Write(buffer, 0, bytesRead);
                        }
                    }  
                    return 0; 
                }
                catch (UnauthorizedAccessException)
                {
                    return -3; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return -4; 
                }
            }
            finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
