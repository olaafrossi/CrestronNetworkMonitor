using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using ThreeByteLibrary.Dotnet;

namespace CrestronNetworkMonitorWPFUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            CreateLocalDirectoryForAppFiles();
            SetupApp();
            InitializeComponent();
            WriteVersionNumberToUI();
        }

        // This is the DI code that works,
        // but i can't seem to find a way to get events back to write to the UI

        public void SetupApp()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            // setup a console logger and logger to the Windows event viewer-
            // note, must run a PS script to build the Windows event viewer log entry
            // figure out a clean way to write and distribute the script

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.EventLog("ThreeByteCrestronNetworkMonitor",
                    "Application", ".", false,
                    "{Message}", restrictedToMinimumLevel: LogEventLevel.Verbose, eventIdProvider: null,
                    formatProvider: null)
                .CreateLogger();

            // write our first log message
            Log.Logger.Information("Application Starting");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IPcNetworkListener, PcNetworkListener>();
                })
                .UseSerilog()
                .Build();

            // launch the class
            var svcPcNetworkListener = ActivatorUtilities.CreateInstance<PcNetworkListener>(host.Services);
            svcPcNetworkListener.Run();
        }

        private static void CreateLocalDirectoryForAppFiles()
        {
            AppSettings jsonSettings = new AppSettings();
            string desiredFolder = Properties.Resources.LocalDataFolder;

            //Ensure the directory exists
            if(Directory.Exists(desiredFolder) is false)
            {
                Directory.CreateDirectory(desiredFolder);
            }

            string file = $"{desiredFolder}appsettings.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            string jsonString = JsonSerializer.Serialize(jsonSettings, options);
            File.WriteAllText(file, jsonString);
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Properties.Resources.LocalDataFolderFile, false, true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    true)
                .AddEnvironmentVariables();
        }

        // maybe do a binding property here instead
        public void WriteLine(string message)
        {
            Dispatcher.Invoke(() =>
            {
                crestronLogText.AppendText($"{message} \n");
                crestronLogText.ScrollToEnd();
            });
        }

        public void WriteVersionNumberToUI()
        {
            // should probably get the current directory to be safe, or wrap in a try-catch

            // and this is crashing. Figure out a way to get this. Reflection?
            FileVersionInfo threeByteLib = FileVersionInfo.GetVersionInfo("ThreeByteLibrary.Dotnet.dll");

            Dispatcher.Invoke(() =>
            {
                appVersionText.Text = $"App Version: {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}, 3Byte Library Version: {threeByteLib.FileVersion}";
            });
        }

        private void SetDefaultData()
        {
            AppSettings jsonSettings = new AppSettings();
            
        }

        public class AppSettings
        {
            public int UdpPort { get; set; } = 16009;
        }
    }
}