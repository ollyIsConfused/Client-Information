using System.Runtime.InteropServices;

namespace ClientInformation.App.Services;

/// <summary>
/// Bettet unser Fenster als Kind des Desktop-WorkerW-Fensters ein,
/// sodass es hinter allen normalen Fenstern, aber vor dem Wallpaper liegt.
/// Technik: identisch mit Wallpaper Engine.
/// </summary>
public static class DesktopWindowService
{
    [DllImport("user32.dll")] private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);
    [DllImport("user32.dll")] private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string? lpszWindow);
    [DllImport("user32.dll")] private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    [DllImport("user32.dll")] private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const uint SMTO_NORMAL = 0x0000;
    private const uint WM_SPAWN_WORKER = 0x052C;

    /// <summary>
    /// Hängt <paramref name="hwnd"/> als Kind in den WorkerW-Bereich des Desktops ein.
    /// Gibt true zurück wenn erfolgreich.
    /// </summary>
    public static bool AttachToDesktop(IntPtr hwnd)
    {
        var progman = FindWindow("Progman", null);
        if (progman == IntPtr.Zero) return false;

        // Progman anweisen, einen WorkerW hinter den Desktop-Icons zu spawnen
        SendMessageTimeout(progman, WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero,
                           SMTO_NORMAL, 1000, out _);

        // Den WorkerW finden, der eine SHELLDLL_DefView als Kind hat
        IntPtr workerW = IntPtr.Zero;

        EnumWindows((hWnd, _) =>
        {
            var shell = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shell != IntPtr.Zero)
            {
                // Der nächste WorkerW-Sibling ist unser Ziel
                workerW = FindWindowEx(IntPtr.Zero, hWnd, "WorkerW", null);
            }
            return true;
        }, IntPtr.Zero);

        if (workerW == IntPtr.Zero) return false;

        SetParent(hwnd, workerW);
        return true;
    }
}
