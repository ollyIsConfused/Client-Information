using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using ClientInformation.App.Models;
using ClientInformation.Data.Models;

namespace ClientInformation.App.Services;

public class SystemInfoService
{
    private readonly AppSettings _settings;

    public SystemInfoService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task<SystemInfo> GetAsync()
    {
        var info = new SystemInfo
        {
            Username    = Environment.UserName,
            PcName      = Environment.MachineName,
            Domain      = Environment.UserDomainName,
            Adapters    = GetNetworkAdapters(),
            EpmCoreHost = _settings.EpmCoreHost,
            LastRefreshed = DateTime.Now
        };

        info.EpmConnection    = await TestTcpConnectionAsync(_settings.EpmCoreHost, _settings.EpmCorePort);
        info.EpmServiceStatus = GetServiceStatus(_settings.EpmServiceName);

        return info;
    }

    // ── Netzwerk ──────────────────────────────────────────────────────────────

    private static List<NetworkAdapterInfo> GetNetworkAdapters()
    {
        var result = new List<NetworkAdapterInfo>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up) continue;
            if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback
                                         or NetworkInterfaceType.Tunnel) continue;

            var ipv4 = nic.GetIPProperties().UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            if (ipv4 is null) continue;

            var mac = string.Join("-",
                nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));

            result.Add(new NetworkAdapterInfo
            {
                AdapterName = nic.Name,
                Description = nic.Description,
                IPv4        = ipv4.Address.ToString(),
                MacAddress  = mac
            });
        }

        return result;
    }

    // ── EPM TCP-Test ──────────────────────────────────────────────────────────

    private static async Task<bool> TestTcpConnectionAsync(string host, int port)
    {
        if (string.IsNullOrWhiteSpace(host)) return false;
        try
        {
            using var client = new TcpClient();
            using var cts    = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await client.ConnectAsync(host, port, cts.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Windows-Service ───────────────────────────────────────────────────────

    private static string GetServiceStatus(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName)) return "—";
        try
        {
            using var sc = new ServiceController(serviceName);
            return sc.Status switch
            {
                ServiceControllerStatus.Running  => "Running",
                ServiceControllerStatus.Stopped  => "Stopped",
                ServiceControllerStatus.Paused   => "Paused",
                ServiceControllerStatus.StartPending => "Starting",
                ServiceControllerStatus.StopPending  => "Stopping",
                _ => sc.Status.ToString()
            };
        }
        catch
        {
            return "Not found";
        }
    }
}
