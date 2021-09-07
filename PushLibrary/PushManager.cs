using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PushLibrary
{
    public static class PushManager
    {
        public static bool IsSupported => true;

        public static async Task<string> CreateChannelAsync(string appId)
        {
            EnsureSingletonRegistered();

            var process = Process.Start(Path.Combine(SingletonDirectory, "PushSingleton.exe"), new string[]
            {
                "-r",
                "-a", appId,
                "-e", Process.GetCurrentProcess().MainModule.FileName
            });

            await process.WaitForExitAsync();

            return "registered";
        }

#if DEBUG
        private static string SingletonDirectory => @"C:\Users\aleader\source\repos\PushSingletonDemo\PushSingleton\bin\Debug\net5.0-windows10.0.19041";
#else
        private static string SingletonDirectory => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PushSingleton");
#endif


        private static void EnsureSingletonRegistered()
        {
            var singletonFolderPath = SingletonDirectory;

            if (!Directory.Exists(singletonFolderPath))
            {
                var assembly = typeof(PushManager).Assembly;

                using (Stream s = assembly.GetManifestResourceStream("PushLibrary.PushSingleton.exe"))
                {
                    Directory.CreateDirectory(singletonFolderPath);

                    using (var fileStream = File.Create(Path.Combine(singletonFolderPath, "PushSingleton.exe")))
                    {
                        s.CopyTo(fileStream);
                    }
                }
                //File.Copy("PushSingleton.exe", Path.Combine(singletonFolderPath, "PushSingleton.exe"));
                //ZipFile.ExtractToDirectory("PushSingleton.msix", singletonFolderPath);
            }
        }
    }
}
