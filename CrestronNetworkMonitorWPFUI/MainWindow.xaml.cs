using System;
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
using System.Diagnostics;

namespace CrestronNetworkMonitorWPFUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //CreateLocalDirectoryForAppFiles();
            InitializeComponent();
            SetupApp();
        }

        public void SetupApp()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

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
            WriteLine($"{DateTime.Now:HH:mm:ss.fff} | Application Starting");

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

            // get the version number of the classlib and write it to to the UI
            string libVersionNumber = Assembly.GetAssembly(typeof(PcNetworkListener))?.GetName().Version.ToString();
            WriteVersionNumberToUI(libVersionNumber);

            svcPcNetworkListener.MessageHit += SvcPcNetworkListener_MessageHit;
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath("C:\\ThreeByteIntermedia\\CrestronNetworkMonitor\\Settings\\")
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile(
                $"appsettings.json.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddEnvironmentVariables();
        }

        private void SvcPcNetworkListener_MessageHit(object sender, PcNetworkListener.PCNetworkListenerMessages e)
        {
            WriteLine(e);
        }

        private static void CreateLocalDirectoryForAppFiles()
        {
            AppSettings jsonSettings = new AppSettings();
            string desiredFolder = Properties.Resources.LocalDataFolder;

            //Ensure the directory exists
            if (Directory.Exists(desiredFolder) is false)
            {
                Directory.CreateDirectory(desiredFolder);
            }

            string file = $"{desiredFolder}appsettings.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString = JsonSerializer.Serialize(jsonSettings, options);
            File.WriteAllText(file, jsonString);
        }

        // maybe do a binding property here instead
        public void WriteLine(PcNetworkListener.PCNetworkListenerMessages e)
        {
            if (e.UILogger == PcNetworkListener.PCNetworkListenerMessages._UiLogger.netLog)
            {
                Dispatcher.Invoke(() =>
                {
                    crestronLogText.AppendText($"{e.Message} \n");
                    crestronLogText.ScrollToEnd();
                });
            }

            if (e.UILogger == PcNetworkListener.PCNetworkListenerMessages._UiLogger.appLog)
            {
                Dispatcher.Invoke(() =>
                {
                    appLogText.AppendText($"{e.Message} \n");
                    appLogText.ScrollToEnd();
                });
            }
        }

        public void WriteLine(string message)
        {
            Log.Logger.Information(message);
            Dispatcher.Invoke(() =>
                {
                    appLogText.AppendText($"{message} \n");
                    appLogText.ScrollToEnd();
                });
        }

        public void WriteVersionNumberToUI(string message)
        {
            Dispatcher.Invoke(() =>
            {
                appVersionText.Text =
                    $"App Version: {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}, 3Byte Library Version: {message}";
            });
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Written by Three Byte www.3-byte.com", "Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}