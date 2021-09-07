using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PushSingleton
{
    class Program
    {
        public class Options
        {
            [Option('r', "registerApp")]
            public bool RegisterApp { get; set; }

            [Option('a', "appId")]
            public string AppId { get; set; }

            [Option('e', "appExe")]
            public string AppExe { get; set; }

            [Option('n', "notifyApp")]
            public bool NotifyApp { get; set; }

            [Option('p', "payload")]
            public string Payload { get; set; }
        }

        static async Task Main(string[] args)
        {
            Options o = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(v => o = v);


            if (o.RegisterApp)
            {
                Console.WriteLine($"Registering app {o.AppId} with EXE {o.AppExe}...");

                RegisterApp(o.AppId, o.AppExe);

                Console.WriteLine($"Registered!");
            }
            else if (o.NotifyApp)
            {
                Console.WriteLine($"Notifying app {o.AppId}...");

                if (GetRegisteredApps().TryGetValue(o.AppId, out string appExe))
                {
                    Console.WriteLine($"Using EXE {appExe}...");

                    Process.Start(appExe);

                    Console.WriteLine("Notified!");
                }
                else
                {
                    Console.WriteLine("App was not registered.");
                }
            }
            else
            {
                Console.WriteLine("Push service 1.0 started...");

                Console.WriteLine("\nRegistered apps...");

                foreach (var app in GetRegisteredApps())
                {
                    Console.WriteLine($"{app.Key}: {app.Value}");
                }
            }

            await Task.Delay(2000);

            Console.WriteLine("Shutting down...");
        }

        private static string RegisteredAppsPath = Path.Combine(Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName, "registeredApps.json");

        private static Dictionary<string, string> GetRegisteredApps()
        {
            try
            {
                var txt = File.ReadAllText(RegisteredAppsPath);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(txt);
            }
            catch { }

            return new Dictionary<string, string>();
        }

        private static void RegisterApp(string appId, string appExe)
        {
            var registeredApps = GetRegisteredApps();
            registeredApps[appId] = appExe;

            File.WriteAllText(RegisteredAppsPath, JsonConvert.SerializeObject(registeredApps));
        }
    }
}
