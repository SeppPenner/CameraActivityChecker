# Analyse: Umstieg von .NET Framework 4.8.1 auf .NET 10

Stand: 2026-08-09, Basis-Commit `f8a81fc`

## Kurzfazit

Ja, der Umstieg ist möglich, und zwar mit überschaubarem Aufwand. Der Code selbst ist fast
unverändert übernehmbar (6 kleine Änderungen), der eigentliche Aufwand steckt in zwei Punkten:

1. **EmguCV 3.1.0.1 (2016) muss auf Emgu.CV 4.13 hoch.** Die alte Version läuft nicht auf .NET 10,
   und die Klasse `Capture` heißt dort inzwischen `VideoCapture`.
2. **Die Auslieferung ändert sich grundlegend.** .NET Framework 4.8.1 ist Teil von Windows, .NET 10
   ist es nicht. Das Inno-Setup muss also entweder die Runtime mitliefern (self-contained) oder sie
   als Voraussetzung installieren.

Der Umbau wurde nicht nur auf dem Papier geprüft: Eine Arbeitskopie des Projekts wurde im
Scratchpad tatsächlich auf `net10.0-windows` umgestellt und gebaut. Ergebnis: **0 Fehler,
0 Warnungen**, inklusive `TreatWarningsAsErrors`. Details unter "Verifikation".

## Ausgangslage

| Punkt | Ist-Zustand |
| --- | --- |
| SDK | `Microsoft.NET.Sdk.WindowsDesktop` |
| TFM | `netframework4.8.1` (unübliche Schreibweise, kanonisch wäre `net481`) |
| UI | WinForms, 2 Forms, P/Invoke auf `user32`/`gdi32` |
| Kameraerkennung | EmguCV 3.1.0.1, `new Capture()` in einer Endlosschleife |
| Lokalisierung | `HaemmerElectronics.SeppPenner.Language` 1.1.2 (eigenes Paket) |
| Versionierung | GitVersion.MsBuild 5.11.1 |
| Setup | Inno Setup, nimmt `bin\publish` aus `dotnet publish` |
| CI | AppVeyor, aber **keine** `appveyor.yml` im Repo (Konfiguration liegt auf der Website) |

## Was der Umstieg konkret erfordert

### 1. Projektdatei

```xml
<Project Sdk="Microsoft.NET.Sdk">          <!-- statt Microsoft.NET.Sdk.WindowsDesktop -->
  <TargetFramework>net10.0-windows</TargetFramework>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>   <!-- statt RuntimeIdentifiers, wegen der nativen DLLs -->
```

Ersatzlos streichbar:

- `System.Resources.Extensions` (Version 7.0.0): steckt bei .NET 10 in der Shared Framework, das
  Paket wird von NuGet ohnehin weggeschnitten.
- `GenerateResourceUsePreserializedResources`: wird ohne dieses Paket nicht mehr gebraucht.
- `OpenTK.dll.config`: gehört zu `Emgu.CV.UI.GL` aus EmguCV 3.1, das mit Emgu.CV 4.x wegfällt.
  Die Datei enthält Mono-`dllmap`-Einträge für Linux und macOS, für dieses Projekt komplett tot.

Mit `RuntimeIdentifier=win-x64` entfällt der bisher mitgelieferte x86-Zweig. Auf Windows 11 ist das
unkritisch, arm64 wäre über einen zweiten Publish-Lauf möglich (Emgu liefert `win-arm64` mit).

### 2. EmguCV 3.1.0.1 zu Emgu.CV 4.13.0.5924

Das Paket wurde aufgeteilt und heißt anders:

| alt | neu |
| --- | --- |
| `EmguCV` 3.1.0.1 (verwaltet + nativ in einem) | `Emgu.CV` 4.13.0.5924 (nur verwaltet, netstandard2.0) |
| | plus `Emgu.CV.runtime.windows` **oder** `Emgu.CV.runtime.mini.windows` (nativ) |

Codeseitig genau zwei Zeilen in `Main.cs`:

```csharp
private VideoCapture capture = new();   // statt: private Capture capture = new Capture();
...
this.capture = new VideoCapture();      // statt: this.capture = new Capture();
```

`QueryFrame()` gibt weiterhin ein `Mat` zurück, der Aufruf bleibt unverändert.

**Zur Wahl des Runtime-Pakets:** `Emgu.CV.runtime.mini.windows` reicht. Geprüft über
`CvInvoke.BuildInformation`, das Mini-Build meldet `Video I/O: DirectShow YES, Media Foundation YES`,
also genau die Backends, über die `VideoCapture(0)` auf Windows an die Kamera geht. Die `cvextern.dll`
ist dort 9,7 MB statt 45,8 MB. Zusätzlich kann die mitgelieferte ffmpeg-DLL (27,3 MB) aus dem
Publish geworfen werden, sie wird nur zum Lesen von Videodateien gebraucht:

```xml
<!-- The ffmpeg native library is only needed to read video files, not for camera capture. -->
<Target Name="RemoveFfmpegFromPublish" AfterTargets="ComputeFilesToPublish">
    <ItemGroup>
        <ResolvedFileToPublish Remove="@(ResolvedFileToPublish)"
                               Condition="$([System.String]::Copy('%(Filename)').StartsWith('opencv_videoio_ffmpeg'))" />
    </ItemGroup>
</Target>
```

### 3. Sprachpaket auf 1.1.6

Das ist der Punkt, der die Richtung ohnehin vorgibt: `HaemmerElectronics.SeppPenner.Language` 1.1.2
hat noch `netstandard2.0` und läuft deshalb heute unter .NET Framework. Ab 1.1.6 gibt es nur noch
`net8.0` und `net9.0`. Das Paket kann auf .NET Framework also gar nicht mehr aktualisiert werden,
unter .NET 10 greift dagegen das `net9.0`-Asset problemlos. Da es das eigene Paket ist, wäre ein
Release 1.1.7 mit `net10.0` der saubere Abschluss, notwendig ist es für die Migration nicht.

### 4. Sechs Codeänderungen wegen `TreatWarningsAsErrors`

Der erste Build gegen net10.0-windows lieferte 5 Fehler. Alle stammen daher, dass die .NET-Core-
Referenzassemblies von WinForms nullable-annotiert sind, die von .NET Framework nicht:

| Datei | Meldung | Fix |
| --- | --- | --- |
| `Notifications/FormAnimator.cs:65` | CS8622 `sender` bei `Form_Load` | `object? sender` |
| `Notifications/FormAnimator.cs:66` | CS8622 `sender` bei `Form_VisibleChanged` | `object? sender` |
| `Notifications/FormAnimator.cs:67` | CS8622 `sender` bei `Form_Closing` | `object? sender` |
| `Notifications/FormAnimator.cs:67` | WFDEV004 `Form.Closing` ist veraltet | `form.FormClosing` plus `FormClosingEventArgs` |
| `Notifications/Notification.cs:86` | CS8602 `Screen.PrimaryScreen` kann null sein | echte Null-Prüfung statt `!` |

Die `object sender` zu `object? sender`-Umstellung betrifft konsequenterweise auch die restlichen
Handler in `Notification.cs` (Zeilen 83, 104, 118, 133, 154, 164, 174, 184), sonst kommen dieselben
Fehler beim nächsten Anfassen wieder hoch.

Für `Screen.PrimaryScreen` wurde im Proof of Concept `!` benutzt, um schnell grün zu werden. In der
echten Umsetzung gehört dort eine Prüfung hin, denn ohne angeschlossenen Monitor ist der Wert
tatsächlich null.

### 5. Optional: `Program.cs` modernisieren

```csharp
ApplicationConfiguration.Initialize();   // ersetzt EnableVisualStyles + SetCompatibleTextRenderingDefault
Application.Run(new Main());
```

Dazu passend `ApplicationHighDpiMode`, `ApplicationVisualStyles` und
`ApplicationUseCompatibleTextRendering` in der csproj. Nicht erforderlich, aber der übliche Weg ab
.NET 5 und die Voraussetzung dafür, DPI-Verhalten sauber zu setzen.

## Verifikation

Arbeitskopie unter `%TEMP%\claude\...\scratchpad\poc`, das Repo selbst wurde nicht angefasst.

- Restore mit Emgu.CV 4.13.0.5924, Emgu.CV.runtime(.mini).windows 4.13.0.5924 und
  Language 1.1.6 gegen `net10.0-windows`: erfolgreich.
- Build `Release` nach den sechs Codeänderungen: **0 Fehler, 0 Warnungen**.
- GitVersion.MsBuild **5.11.1 läuft unter SDK 10.0.302** und stempelt die Version korrekt. Ein
  Upgrade auf 6.8.2 ist möglich, aber kein Blocker (Achtung: GitVersion 6 hat ein geändertes
  Konfigurationsformat und andere Defaults).
- Vier Publish-Varianten gebaut und vermessen.

Nicht geprüft, weil dafür eine echte Kamera und ein manueller Test nötig sind:

- ob die Kamerakennung mit Emgu 4.13 zur Laufzeit dasselbe Verhalten zeigt wie mit 3.1,
- ob das Inno-Setup durchläuft (Inno Setup war hier nicht aufrufbar).

### Größenvergleich der Publish-Varianten

| Variante | Dateien | Größe |
| --- | --- | --- |
| heute: net481, EmguCV 3.1, Runtime ist Teil von Windows | 23 | 55,4 MB |
| net10, Emgu voll, framework-abhängig | 21 | 76,3 MB |
| net10, Emgu **mini** ohne ffmpeg, framework-abhängig | 19 | **12,8 MB** |
| net10, Emgu mini ohne ffmpeg, self-contained | 285 | 129,7 MB |
| net10, Emgu voll, self-contained | 287 | 193,2 MB |

Die 118 MB Grundlast der self-contained-Varianten sind die WinForms-Runtime selbst. Trimming
(`PublishTrimmed`) hilft nicht, WinForms unterstützt es nicht.

## Der eigentliche Knackpunkt: Auslieferung

`bin\publish` landet per `Source: "..\src\CameraActivityChecker\bin\publish\*"` im Setup. Bisher
funktioniert das ohne Zutun, weil .NET Framework 4.8.1 auf jedem aktuellen Windows vorhanden ist.
Bei .NET 10 gibt es zwei Wege:

**Variante A: self-contained, Runtime im Setup (empfohlen)**

- `dotnet publish -c Release -r win-x64 --self-contained true -o bin/publish`
- Setup-Nutzlast rund 130 MB, nach LZMA-Kompression deutlich weniger.
- Keine Voraussetzung beim Anwender, keine Fehlerquelle beim ersten Start.
- Sicherheitsupdates der Runtime kommen nur über ein neues Setup.

**Variante B: framework-abhängig plus Runtime-Voraussetzung**

- Nutzlast nur 12,8 MB.
- Das `.iss` braucht einen Check auf die installierte .NET Desktop Runtime 10 und im Zweifel einen
  Download-Schritt (Inno Setup 6.3 kann das über `DownloadTemporaryFile`).
- Dafür bleibt die Runtime über Windows Update aktuell.

Für ein kleines Hintergrundwerkzeug ist Variante A die pragmatischere Wahl, zumal das heutige Setup
mit 55 MB Nutzlast auch nicht klein ist.

Weitere Anpassungen an der Auslieferung:

- `Setup\build-setup-files.bat`: `dotnet publish` braucht künftig `-r win-x64` und je nach Variante
  `--self-contained true|false`.
- `Setup\CameraActivityChecker-Setup.iss`: Version hochziehen, bei Variante A zusätzlich
  `ArchitecturesInstallIn64BitMode=x64compatible`, bei Variante B den Runtime-Check ergänzen.
- Die `.iss` ist derzeit nicht UTF-8 kodiert, im Editor steht dort `H�mmer Electronics`. Inno Setup 6
  ist Unicode-only, die Datei sollte bei der Gelegenheit als UTF-8 mit BOM gespeichert werden.
- AppVeyor: das Build-Image muss das .NET 10 SDK mitbringen. Da keine `appveyor.yml` im Repo liegt,
  ist das auf der AppVeyor-Website nachzuziehen. Eine `appveyor.yml` einzuchecken wäre ohnehin
  sinnvoll, dann ist der CI-Stand versioniert.

## Was ausdrücklich kein Problem ist

- **resx-Dateien.** Die `mimetype="application/x-microsoft.net.object.binary.base64"`-Treffer in
  `Main.resx` und `Notification.resx` stehen im Kommentarblock der Vorlage, nicht in echten Einträgen.
  Die tatsächlichen Ressourcen (`$this.Icon`, `$this.BackgroundImage`) laufen über TypeConverter, nicht
  über den BinaryFormatter. Der in .NET 9 entfernte BinaryFormatter ist hier also kein Thema, was der
  grüne Build bestätigt.
- **P/Invoke.** `GetForegroundWindow`, `SetForegroundWindow`, `AnimateWindow`, `CreateRoundRectRgn`
  funktionieren unter .NET 10 unverändert.
- **WinForms-Designer-Dateien.** Kein Fremdsteuerelement im Designer, `Main.Designer.cs` und
  `Notification.designer.cs` enthalten nur Bordmittel.
- **`.editorconfig`.** Die Regeln (file scoped namespaces, usings im Namespace, `this.`-Qualifizierung)
  passen unverändert weiter.
- **GitVersion 5.11.1.** Läuft, siehe oben.

Randnotiz zur Build-Umgebung: Auf diesem Rechner steht ein Telerik-Feed in einer globalen
NuGet-Konfiguration, der mit 404 antwortet und den Restore abbricht. Das ist kein Projektproblem,
eine projektlokale `NuGet.config` mit `<clear />` und nuget.org umgeht es zuverlässig.

## Offene Punkte für die Umsetzung

1. **Smoke-Test mit echter Kamera.** Emgu 4.13 nutzt andere Capture-Backends als 3.1. Zu prüfen ist,
   ob `VideoCapture` beim inaktiven Zustand wie bisher kein Frame liefert.
2. **Lizenz.** Emgu CV ist in 3.1 wie in 4.13 unter GPL v3 dual lizenziert, das Projekt selbst steht
   unter MIT. Das ist ein bestehender Zustand, den die Migration weder verbessert noch verschlechtert,
   aber ein Grund mehr, sich Punkt 2 der Empfehlungen anzusehen.

## Empfehlungen über die reine Migration hinaus

Diese Punkte gehören nicht zum .NET-10-Umstieg, fallen bei der Arbeit am Code aber sofort auf.

**1. Die Anwendung erreicht ihre Nachrichtenschleife nie.**
`Application.Run(new Main())` kommt nicht zum Zug, weil der Konstruktor von `Main` über
`CheckCameraActivated()` in eine `while (true)`-Schleife läuft und nie zurückkehrt. Die
Benachrichtigungen werden also ohne laufende Message-Pump angezeigt, dazwischen steht ein
`Thread.Sleep(2000)` im UI-Thread. Sauber wäre ein `System.Windows.Forms.Timer` oder ein
Hintergrundtask plus `NotifyIcon`. Dazu passt, dass pro Durchlauf ein neues `VideoCapture` erzeugt
und mit `GC.Collect()` plus `GC.WaitForPendingFinalizers()` hinterhergeräumt wird.

**2. Die Kameraerkennung könnte ohne EmguCV auskommen.**
Aktuell wird die Kamera geöffnet, um festzustellen, ob die Kamera geöffnet ist. Windows führt seit
Windows 10 selbst Buch darüber, unter
`HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam`
(plus `\NonPackaged` für klassische Programme). Jeder Unterschlüssel hat `LastUsedTimeStop`, der Wert
`0` bedeutet "greift gerade zu". Auf diesem Rechner ist der Schlüssel vorhanden und gefüllt, der Weg
funktioniert also. Das würde bedeuten:

- kein EmguCV, keine 130 MB, keine GPL-Frage,
- kein Einschalten der eigenen Kamera nur zum Prüfen,
- als Bonus die Information, **welches** Programm die Kamera benutzt.

Das ist ein größerer Umbau als die Migration und sollte davon getrennt bleiben.

## Vorgeschlagene Reihenfolge

1. Branch anlegen, `net10.0-windows`, SDK und Pakete umstellen, `Capture` zu `VideoCapture`.
2. Die sechs Nullability- und Obsolete-Stellen abarbeiten, Build muss mit
   `TreatWarningsAsErrors` grün sein.
3. `System.Resources.Extensions` und `OpenTK.dll.config` entfernen.
4. Smoke-Test mit echter Kamera, ein- und ausschalten.
5. Publish-Variante festlegen (A oder B), `build-setup-files.bat` und `.iss` anpassen, Setup bauen und
   auf einem Rechner ohne .NET 10 testen.
6. AppVeyor auf das .NET 10 SDK heben, idealerweise mit eingecheckter `appveyor.yml`.
7. `Changelog.md` und `README.md` aktualisieren, Version auf 1.1.0.0.

## Aufwand

| Schritt | Aufwand |
| --- | --- |
| Schritte 1 bis 3 (Code und Projektdatei) | rund 1 Stunde, im Proof of Concept bereits durchgespielt |
| Schritt 4 (Kameratest) | 15 Minuten, kann Nacharbeit auslösen, falls Emgu 4 anders reagiert |
| Schritt 5 (Setup) | 1 bis 3 Stunden, je nach Variante |
| Schritte 6 und 7 | 30 Minuten |
