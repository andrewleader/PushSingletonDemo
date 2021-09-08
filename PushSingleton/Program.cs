using CommandLine;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
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
                using (var mutex = new Mutex(false, "PushSingleton"))
                {
                    if (!mutex.WaitOne(TimeSpan.Zero))
                    {
                        Console.WriteLine("Already running...");
                        return;
                    }

                    Console.WriteLine("Push service 1.0 starting...");

                    var connection = new HubConnectionBuilder()
                        .WithUrl("https://pushsingletondemo.azurewebsites.net/pushHub")
                        .Build();

                    await connection.StartAsync();

                    Console.WriteLine("Push service 1.0 started! Waiting for messages...");

                    connection.On<string, string>("ReceivePush", (appId, message) =>
                    {
                        Console.WriteLine("Push received [" + appId + "]: " + message);

                        try
                        {
                            NotifyApp(appId, message);
                        }
                        catch { }
                    });

                    connection.Closed += async (error) =>
                    {
                        Console.WriteLine("Reconnecting...");
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        await connection.StartAsync();
                    };

                    while (true)
                    {
                        await Task.Delay(1000);
                    }
                }

                Console.WriteLine("\nRegistered apps...");

                foreach (var app in GetRegisteredApps())
                {
                    Console.WriteLine($"{app.Key}: {app.Value}");
                }
            }

            Console.WriteLine("Shutting down...");
        }

        private static void NotifyApp(string appId, string message)
        {
            if (GetRegisteredApps().TryGetValue(appId, out string appExe))
            {
                Console.WriteLine($"Using EXE {appExe}...");

                try
                {
                    Process.Start(appExe, new string[] { "-pushNotification", message });
                    Console.WriteLine("Notified!");
                }
                catch { Console.WriteLine("Failed to notify"); }

            }
            else
            {
                Console.WriteLine("App was not registered.");
            }
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
