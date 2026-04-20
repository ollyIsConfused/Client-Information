using ClientInformation.Shared.Constants;

namespace ClientInformation.TrayAgent;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatalError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ShowFatalError(e.ExceptionObject as Exception);

        using var mutex = new Mutex(true, AppConstants.TrayMutexName, out var isNewInstance);
        if (!isNewInstance) return;

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
        catch (Exception ex)
        {
            ShowFatalError(ex);
        }
    }

    private static void ShowFatalError(Exception? ex)
    {
        var msg = ex is null ? "Unbekannter Fehler." : $"{ex.GetType().Name}:\n{ex.Message}\n\n{ex.StackTrace}";
        MessageBox.Show(msg, "ClientInformation TrayAgent — Fehler",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
