﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CrestronNetworkMonitorWPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThreadPool.QueueUserWorkItem(ListenLoop);
            var log = appLogText; // set logger context
            WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} {DateTime.Now.Date.ToString("d")} Application loop starting ");
        }

        public void WriteLine(TextBox log, string format, params object[] args)
        {
            string formattedStr = null;
            try
            {
                formattedStr = string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (Exception e)
            {
                Dispatcher.Invoke(() =>
                {
                    log.AppendText($"{e.Message} \n");
                    log.ScrollToEnd();
                });
            }

            if (formattedStr != null)
            {
                Trace.WriteLine(formattedStr);

                Dispatcher.Invoke(() =>
                {
                    log.AppendText($"{formattedStr} \n");
                    log.ScrollToEnd();
                });
            }
        }

        public static readonly int UDP_LISTEN_PORT = 16009;

        private void ListenLoop(object state)
        {
            // Open a UDP listener on port 16009
            UdpClient udpClient = new UdpClient(UDP_LISTEN_PORT);
            
            bool listening = true;
            IPEndPoint remoteHost = new IPEndPoint(IPAddress.Any, 0);
            byte[] dataBytes;

            var log = appLogText;
            WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} UDP listener started on port {UDP_LISTEN_PORT}");

            while(listening) 
            {
                log = appLogText; 
                WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Last Remote: {remoteHost.Address.ToString()} on Port: {remoteHost.Port}");
                dataBytes = udpClient.Receive(ref remoteHost);
                log = crestronLogText;
                WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Received Raw String: {dataBytes.Length} Bytes: {Encoding.ASCII.GetString(dataBytes)}");
                
                string stringIn = Encoding.ASCII.GetString(dataBytes); // Incoming commands must be received as a single packet.
                stringIn = stringIn.ToUpper(); // format the string to upper case for matching

                //Parse messages separated by cr
                int delimPos = stringIn.IndexOf("\r");
                while(delimPos >= 0) 
                {
                    string message = stringIn.Substring(0, delimPos + 1).Trim();
                    stringIn = stringIn.Remove(0, delimPos + 1);  //remove the message
                    delimPos = stringIn.IndexOf("\r");

                    log = crestronLogText;
                    WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Message: {message}");

                    if(message == "EXIT") 
                    {
                        listening = false;
                    } 
                    else if(message == "PING") 
                    {
                        string responseString = "PONG\r";
                        byte[] sendBytes = Encoding.ASCII.GetBytes(responseString);
                        udpClient.Send(sendBytes, sendBytes.Length, remoteHost);
                        log = appLogText;
                        WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Sent: {responseString}");
                    } 
                    else if(message == "APPRESTART") 
                    {
                        log = appLogText;
                        WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Restarting App");
                        RaiseAppRestart();
                    } 
                    else if(message == "REBOOT" || message == "RESTART") 
                    {
                        log = appLogText;
                        WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Rebooting PC");
                        System.Diagnostics.Process.Start("shutdown", "/r /f /t 3 /c \"Reboot Triggered\" /d p:0:0");
                    } 
                    else if(message == "SHUTDOWN") 
                    {
                        log = appLogText;
                        WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Shutting Down PC");
                        System.Diagnostics.Process.Start("shutdown", "/s /f /t 3 /c \"Shutdown Triggered\" /d p:0:0");
                    } 
                    else if (message == "SLEEP") 
                    {
                        log = appLogText;
                        WriteLine(log, $"{DateTime.Now.ToString("HH:mm:ss.fff")} Sleeping PC");
                        System.Diagnostics.Process.Start("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                    }
                }
            }
        }

        public event EventHandler AppRestart;

        private void RaiseAppRestart() 
        {
            if(AppRestart != null) 
            {
                AppRestart(this, EventArgs.Empty);
            }
        }
    }
}
