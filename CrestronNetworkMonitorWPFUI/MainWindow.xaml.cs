using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using ThreeByteLibrary.Dotnet.Standard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CrestronNetworkMonitorWPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

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

            // launch the WPF window
            InitializeComponent();

            string happy = svcPcNetworkListener.userMessages.Message;
            WriteLine(happy);
        }

        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                        optional: true)
                .AddEnvironmentVariables();
        }

        private void UserMessages_MessageChanged(object sender, PcNetworkListener.CrestronAppMessages e)
        {
            //WriteLine(svcPc)
        }

        public void WriteLine(string message)
        {
            
            Dispatcher.Invoke(() =>
                {
                    crestronLogText.AppendText($"{message} \n");
                    crestronLogText.ScrollToEnd();
                });

        }
    }
}

