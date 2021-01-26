using System;
using System.IO;
using Newtonsoft.Json;


namespace ConsoleJsonParse
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string jsonString = File.ReadAllText(@"C:\ThreeByteIntermedia\CrestronNetworkMonitor\Settings\appsettings.json");
            Console.WriteLine(jsonString);

            //AppSettings app = new AppSettings();

            using (StreamReader r = new StreamReader(@"C:\ThreeByteIntermedia\CrestronNetworkMonitor\Settings\appsettings.json"))
            {
                string json = r.ReadToEnd();
                AppSettings app = JsonConvert.DeserializeObject<AppSettings>(json);
                Console.WriteLine("here we go");
                Console.WriteLine(app.UdpPort.ToString());
                app.UdpPort++;
                Console.WriteLine(app.UdpPort.ToString());
            }

            


            //weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(jsonString);

        }
    }
}
