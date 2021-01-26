
public class AppSettings
{
    public string[] exclude { get; set; }
    public int UdpPort { get; set; }
    public Serilog Serilog { get; set; }
}

public class Serilog
{
    public Minimumlevel MinimumLevel { get; set; }
}

public class Minimumlevel
{
    public string Default { get; set; }
    public Override Override { get; set; }
}

public class Override
{
    public string Microsoft { get; set; }
    public string System { get; set; }
}
