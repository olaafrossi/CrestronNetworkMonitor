namespace CrestronNetworkMonitorWPFUI
{

    public class AppSettings
    {
        public string[] exclude { get; set; }
        public int UdpPort { get; set; } = 16019;
        public Serilog Serilog { get; set; }
    }

    public class Serilog
    {
        public Minimumlevel MinimumLevel { get; set; }
    }

    public class Minimumlevel
    {
        public string Default { get; set; } = "Information";
        public Override Override { get; set; }
    }

    public class Override
    {
        public string Microsoft { get; set; } = "Information";
        public string System { get; set; } = "Warning";
    }

}