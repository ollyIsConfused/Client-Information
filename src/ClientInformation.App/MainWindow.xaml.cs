using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ClientInformation.App.Services;
using ClientInformation.Data.Services;

namespace ClientInformation.App;

public partial class MainWindow : Window
{
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_NOZORDER   = 0x0004;

    private readonly SystemInfoService _sysInfo;
    private readonly LoggingService    _logger;
    private System.Threading.Timer?    _refreshTimer;

    public MainWindow()
    {
        InitializeComponent();
        _logger  = new LoggingService();
        _sysInfo = new SystemInfoService(LoadSettings());
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // 1. Daten laden damit der Inhalt seine finale Größe hat
        await RefreshAsync();

        // 2. Layout-Pass abwarten
        UpdateLayout();

        // 3. Ins Desktop-WorkerW einhängen
        var hwnd = new WindowInteropHelper(this).Handle;
        var ok   = DesktopWindowService.AttachToDesktop(hwnd);
        _logger.Info(ok ? "Desktop-Modus aktiv." : "Desktop-Modus fehlgeschlagen.");

        // 4. Position per Win32 setzen (nach SetParent sind WPF Left/Top unzuverlässig)
        PositionBottomRight(hwnd);

        // 5. Auto-Refresh alle 60 Sekunden
        _refreshTimer = new System.Threading.Timer(
            async _ => await Dispatcher.InvokeAsync(async () =>
            {
                await RefreshAsync();
                UpdateLayout();
                PositionBottomRight(new WindowInteropHelper(this).Handle);
            }),
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(60));
    }

    private void PositionBottomRight(IntPtr hwnd)
    {
        var dpi    = VisualTreeHelper.GetDpi(this);
        var screen = SystemParameters.WorkArea;

        // Pixel-Größe aus DPI-skalierten WPF-Maßen berechnen
        var wPx = (int)(ActualWidth  * dpi.DpiScaleX);
        var hPx = (int)(ActualHeight * dpi.DpiScaleY);
        var xPx = (int)((screen.Right  - ActualWidth  - 20) * dpi.DpiScaleX);
        var yPx = (int)((screen.Bottom - ActualHeight - 20) * dpi.DpiScaleY);

        SetWindowPos(hwnd, IntPtr.Zero, xPx, yPx, wPx, hPx, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    private async Task RefreshAsync()
    {
        try
        {
            var info = await Task.Run(() => _sysInfo.GetAsync());

            TxtUsername.Text = info.Username;
            TxtPcName.Text   = info.PcName;
            TxtDomain.Text   = info.Domain;

            AdapterList.ItemsSource = info.Adapters;

            TxtEpmCore.Text = info.EpmCoreHost;

            TxtEpmConnection.Text       = info.EpmConnection ? "True" : "False";
            TxtEpmConnection.Foreground = info.EpmConnection
                ? new SolidColorBrush(Color.FromRgb(0x6B, 0xD4, 0x6B))
                : new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));

            TxtEpmService.Text       = info.EpmServiceStatus;
            TxtEpmService.Foreground = info.EpmServiceStatus == "Running"
                ? new SolidColorBrush(Color.FromRgb(0x6B, 0xD4, 0x6B))
                : new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));

            TxtTimestamp.Text = $"Aktualisiert: {info.LastRefreshed:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            _logger.Error("Fehler beim Abrufen der Systemdaten", ex);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer?.Dispose();
        base.OnClosed(e);
    }

    private static Data.Models.AppSettings LoadSettings()
    {
        try
        {
            var storage = new JsonStorageService();
            var service = new SettingsService(storage);
            return service.LoadAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return new Data.Models.AppSettings();
        }
    }
}
