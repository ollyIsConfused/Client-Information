# ClientInformationSuite

Windows-Desktop-Lösung bestehend aus Haupt-App (WPF) und System-Tray-Agent (WinForms).

## Voraussetzungen

- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Windows 10 / 11

## Projekte

| Projekt | Typ | Aufgabe |
|---|---|---|
| `ClientInformation.App` | WPF App | Hauptfenster mit Kundendaten-Verwaltung |
| `ClientInformation.TrayAgent` | WinForms App | System-Tray-Icon, Prozessüberwachung, Autostart |
| `ClientInformation.Shared` | Class Library | Konstanten, Pfade, gemeinsame Modelle |
| `ClientInformation.Data` | Class Library | JSON-Speicherung, Settings, Logging |

## Datenablage

Alle Benutzerdaten werden gespeichert unter:

```
%AppData%\ClientInformation\
  settings.json
  clients.json
  logs\YYYY-MM-DD.log
```

## Build

```bash
dotnet build ClientInformationSuite.sln
```

## Starten

**TrayAgent** (empfohlen — startet die App über das Tray-Menü):

```bash
dotnet run --project src/ClientInformation.TrayAgent
```

**Haupt-App direkt**:

```bash
dotnet run --project src/ClientInformation.App
```

## TrayAgent — Kontextmenü

Rechtsklick auf das Tray-Icon:

| Menüpunkt | Funktion |
|---|---|
| Öffnen / Fokussieren | App starten oder in den Vordergrund holen |
| Starten | App starten (nur wenn nicht läuft) |
| Neu starten | App beenden und neu starten |
| Autostart aktivieren / deaktivieren | Registry-Eintrag für Benutzer-Autostart |
| App beenden | Haupt-App-Prozess beenden |
| TrayAgent beenden | TrayAgent selbst beenden |

## Single Instance

Die Haupt-App läuft nur einmal. Ein zweiter Start bringt das bestehende Fenster in den Vordergrund.

## Autostart

Der TrayAgent trägt sich selbst in `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` ein.  
`StartMainAppWithWindows` in `settings.json` steuert, ob die Haupt-App automatisch mitstartet.

## Nächste Schritte

- Icon `Client.ico` unter `src/ClientInformation.App/Assets/` und `src/ClientInformation.TrayAgent/Assets/` ablegen
- Installer mit WiX oder NSIS unter `installer/` erstellen
- MVVM weiter ausbauen (MainViewModel als DataContext nutzen)
- Kundendaten um weitere Felder (E-Mail, Telefon, Firma) erweitern
