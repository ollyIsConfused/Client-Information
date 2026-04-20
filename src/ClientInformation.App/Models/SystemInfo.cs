namespace ClientInformation.App.Models;

public class NetworkAdapterInfo
{
    public string AdapterName  { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;
    public string IPv4         { get; set; } = string.Empty;
    public string MacAddress   { get; set; } = string.Empty;
}

public class SystemInfo
{
    public string   Username          { get; set; } = string.Empty;
    public string   PcName            { get; set; } = string.Empty;
    public string   Domain            { get; set; } = string.Empty;

    public List<NetworkAdapterInfo> Adapters { get; set; } = [];

    public string   EpmCoreHost       { get; set; } = string.Empty;
    public bool     EpmConnection     { get; set; }
    public string   EpmServiceStatus  { get; set; } = string.Empty;

    public DateTime LastRefreshed     { get; set; } = DateTime.Now;
}
