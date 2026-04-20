using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientInformation.App.ViewModels;

/// <summary>
/// Basis-ViewModel mit INotifyPropertyChanged.
/// Kann als Basis für spätere MVVM-Erweiterungen genutzt werden.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string _statusText = "Bereit";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }
}
