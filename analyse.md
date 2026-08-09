# Analysis: Migration from .NET Framework 4.8.1 to .NET 10

As of 2026-08-09, base commit `f8a81fc`

## Summary

Yes, the migration is possible, and the effort is manageable. The code itself can be carried over
almost unchanged (6 small changes), the actual work sits in two places:

1. **EmguCV 3.1.0.1 (2016) has to move to Emgu.CV 4.13.** The old version does not run on .NET 10,
   and the class `Capture` has since been renamed to `VideoCapture`.
2. **Distribution changes fundamentally.** .NET Framework 4.8.1 is part of Windows, .NET 10 is not.
   So the Inno Setup either has to ship the runtime (self-contained) or install it as a prerequisite.

The migration was not only checked on paper: a working copy of the project was actually switched to
`net10.0-windows` in the scratchpad and built. Result: **0 errors, 0 warnings**, with
`TreatWarningsAsErrors` enabled. Details under "Verification".

## What was implemented in the end

This section was added after the analysis was acted upon. The rest of the document is kept as it was
written, it documents why the decisions were made.

The move to .NET 10 was done as described. The smoke test with a real camera then produced a result
that changed the plan, see "Camera detection: what the smoke test showed" below: the camera detection
does not use EmguCV any more at all. The dependency was dropped and replaced by the capability access
data of Windows, which is the option that was described under "Recommendations beyond the migration
itself". Everything about Emgu.CV 4.13 in this document therefore describes a step that was taken and
then removed again, it is kept because it explains how the state of the detection was found out.

What is in the code now:

- `CameraUsageDetector` reads the consent store of Windows, no camera access of its own.
- `Main` uses a `System.Windows.Forms.Timer` instead of the endless loop in the constructor, so the
  message loop of the application is actually reached. The main form stays hidden.
- The notification closes through its own life timer, the `Thread.Sleep` on the UI thread is gone.
- The window region of the notification is set through `SetWindowRgn` after the form is shown, see
  "The notification was never visible".

Without the Emgu dependency the publish output is roughly 1 MB framework-dependent instead of the
12.8 MB measured with the mini runtime, so the distribution question below gets easier, not harder.

## Camera detection: what the smoke test showed

The check `IsCameraActivated()` opened the camera and returned whether a frame could be grabbed. Both
stacks were measured against a real camera, once with the camera free and once with the camera held by
another process:

| Camera state | old: EmguCV 3.1 | new: Emgu.CV 4.13 |
| --- | --- | --- |
| free | frame 640x480, result **true** | frame 640x480, result **true** |
| held by another process | empty 0x0 frame, not null, result **true** | frame is null, result **false** |

Two conclusions came out of this:

1. **With EmguCV 3.1 the application never detected anything.** The result was always true, so the
   state flipped once at startup and never again. Exactly one notification was shown, at startup.
2. **With Emgu.CV 4.13 the states are told apart, but the meaning is inverted.** A free camera counted
   as activated, a camera in use by another program counted as deactivated.

On top of that, the check opened the camera roughly every 1.7 seconds, which keeps the camera busy and
its indicator light on, and can take the camera away from other programs. That is what made the
registry based detection the fix instead of just inverting the boolean.

## The notification was never visible

While verifying the new detection, the notifications turned out not to appear on screen although the
window was created, positioned correctly and reported as visible. `GetWindowRgn` showed the reason: the
window was clipped to a region of 46x14 pixels.

The cause is `AnimateWindow`, which uses the window region itself for the animation and overwrites a
region that was set before it runs. The line in the constructor of `Notification` therefore had no
lasting effect and what remained was a frozen frame of the animation. The region is now applied
through `SetWindowRgn` in the `Shown` handler, which is after the animation has finished.

This is not a migration damage, it is an old defect. It never showed up because the application never
got as far as displaying a notification.

## Starting point

| Item | Current state |
| --- | --- |
| SDK | `Microsoft.NET.Sdk.WindowsDesktop` |
| TFM | `netframework4.8.1` (unusual spelling, `net481` would be canonical) |
| UI | WinForms, 2 forms, P/Invoke into `user32`/`gdi32` |
| Camera detection | EmguCV 3.1.0.1, `new Capture()` inside an endless loop |
| Localization | `HaemmerElectronics.SeppPenner.Language` 1.1.2 (own package) |
| Versioning | GitVersion.MsBuild 5.11.1 |
| Setup | Inno Setup, picks up `bin\publish` from `dotnet publish` |
| CI | AppVeyor, but **no** `appveyor.yml` in the repository (configuration lives on the website) |

## What the migration actually requires

### 1. Project file

```xml
<Project Sdk="Microsoft.NET.Sdk">          <!-- instead of Microsoft.NET.Sdk.WindowsDesktop -->
  <TargetFramework>net10.0-windows</TargetFramework>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>   <!-- instead of RuntimeIdentifiers, because of the native libraries -->
```

Can be dropped without replacement:

- `System.Resources.Extensions` (version 7.0.0): part of the shared framework on .NET 10, NuGet
  prunes the package anyway.
- `GenerateResourceUsePreserializedResources`: no longer needed once that package is gone.
- `OpenTK.dll.config`: belongs to `Emgu.CV.UI.GL` from EmguCV 3.1, which disappears with Emgu.CV 4.x.
  The file contains Mono `dllmap` entries for Linux and macOS, completely dead weight for this project.

With `RuntimeIdentifier=win-x64` the x86 branch that is shipped today goes away. That is uncritical on
Windows 11, and arm64 would be possible through a second publish run (Emgu ships `win-arm64`).

### 2. EmguCV 3.1.0.1 to Emgu.CV 4.13.0.5924

The package was split up and is named differently:

| old | new |
| --- | --- |
| `EmguCV` 3.1.0.1 (managed plus native in one) | `Emgu.CV` 4.13.0.5924 (managed only, netstandard2.0) |
| | plus `Emgu.CV.runtime.windows` **or** `Emgu.CV.runtime.mini.windows` (native) |

On the code side exactly two lines in `Main.cs`:

```csharp
private VideoCapture capture = new();   // was: private Capture capture = new Capture();
...
this.capture = new VideoCapture();      // was: this.capture = new Capture();
```

`QueryFrame()` still returns a `Mat`, so the call site stays unchanged.

**On the choice of the runtime package:** `Emgu.CV.runtime.mini.windows` is enough. Checked through
`CvInvoke.BuildInformation`, the mini build reports `Video I/O: DirectShow YES, Media Foundation YES`,
which are exactly the backends `VideoCapture(0)` uses to reach the camera on Windows. Its
`cvextern.dll` is 9.7 MB instead of 45.8 MB. On top of that, the bundled ffmpeg library (27.3 MB) can
be removed from the publish output, it is only needed to read video files:

```xml
<!-- The ffmpeg native library is only needed to read video files, not for camera capture. -->
<Target Name="RemoveFfmpegFromPublish" AfterTargets="ComputeFilesToPublish">
    <ItemGroup>
        <ResolvedFileToPublish Remove="@(ResolvedFileToPublish)"
                               Condition="$([System.String]::Copy('%(Filename)').StartsWith('opencv_videoio_ffmpeg'))" />
    </ItemGroup>
</Target>
```

### 3. Language package to 1.1.6

This is the point that sets the direction anyway: `HaemmerElectronics.SeppPenner.Language` 1.1.2 still
has `netstandard2.0` and therefore runs on .NET Framework today. From 1.1.6 on there is only `net8.0`
and `net9.0`. So the package can no longer be updated on .NET Framework at all, while on .NET 10 the
`net9.0` asset applies without trouble. Since it is an own package, a 1.1.7 release with `net10.0`
would be the clean finish, but it is not required for the migration.

### 4. Six code changes because of `TreatWarningsAsErrors`

The first build against net10.0-windows produced 5 errors. All of them come from the fact that the
.NET Core reference assemblies of WinForms are nullable annotated while the .NET Framework ones are not:

| File | Message | Fix |
| --- | --- | --- |
| `Notifications/FormAnimator.cs:65` | CS8622 `sender` on `Form_Load` | `object? sender` |
| `Notifications/FormAnimator.cs:66` | CS8622 `sender` on `Form_VisibleChanged` | `object? sender` |
| `Notifications/FormAnimator.cs:67` | CS8622 `sender` on `Form_Closing` | `object? sender` |
| `Notifications/FormAnimator.cs:67` | WFDEV004 `Form.Closing` is obsolete | `form.FormClosing` plus `FormClosingEventArgs` |
| `Notifications/Notification.cs:86` | CS8602 `Screen.PrimaryScreen` may be null | proper null check instead of `!` |

Consequently, the `object sender` to `object? sender` change also affects the remaining handlers in
`Notification.cs` (lines 83, 104, 118, 133, 154, 164, 174, 184), otherwise the same errors come back
the next time somebody touches the file.

For `Screen.PrimaryScreen` the proof of concept used `!` to get to green quickly. The real
implementation should have a check there, because without an attached monitor the value really is null.

### 5. Optional: modernize `Program.cs`

```csharp
ApplicationConfiguration.Initialize();   // replaces EnableVisualStyles plus SetCompatibleTextRenderingDefault
Application.Run(new Main());
```

Together with `ApplicationHighDpiMode`, `ApplicationVisualStyles` and
`ApplicationUseCompatibleTextRendering` in the csproj. Not required, but the usual way since .NET 5 and
the precondition for setting the DPI behaviour properly.

## Verification

Working copy under `%TEMP%\claude\...\scratchpad\poc`, the repository itself was left untouched.

- Restore with Emgu.CV 4.13.0.5924, Emgu.CV.runtime(.mini).windows 4.13.0.5924 and Language 1.1.6
  against `net10.0-windows`: successful.
- `Release` build after the six code changes: **0 errors, 0 warnings**.
- GitVersion.MsBuild **5.11.1 runs under SDK 10.0.302** and stamps the version correctly. An upgrade
  to 6.8.2 is possible but not a blocker (note: GitVersion 6 has a changed configuration format and
  different defaults).
- Four publish variants built and measured.

Not checked, because that needs a real camera and a manual test:

- whether camera detection with Emgu 4.13 behaves at runtime the same way it does with 3.1,
- whether the Inno Setup builds (Inno Setup could not be invoked here).

### Size comparison of the publish variants

| Variant | Files | Size |
| --- | --- | --- |
| today: net481, EmguCV 3.1, runtime is part of Windows | 23 | 55.4 MB |
| net10, full Emgu, framework-dependent | 21 | 76.3 MB |
| net10, **mini** Emgu without ffmpeg, framework-dependent | 19 | **12.8 MB** |
| net10, mini Emgu without ffmpeg, self-contained | 285 | 129.7 MB |
| net10, full Emgu, self-contained | 287 | 193.2 MB |

The 118 MB base load of the self-contained variants is the WinForms runtime itself. Trimming
(`PublishTrimmed`) does not help, WinForms does not support it.

## The real sticking point: distribution

`bin\publish` ends up in the setup through `Source: "..\src\CameraActivityChecker\bin\publish\*"`.
So far this works without any extra step, because .NET Framework 4.8.1 is present on every current
Windows. With .NET 10 there are two ways:

**Variant A: self-contained, runtime inside the setup (recommended)**

- `dotnet publish -c Release -r win-x64 --self-contained true -o bin/publish`
- Setup payload around 130 MB, considerably less after LZMA compression.
- No prerequisite on the user side, no failure mode on first start.
- Runtime security updates only arrive through a new setup.

**Variant B: framework-dependent plus runtime prerequisite**

- Payload of only 12.8 MB, and roughly 1 MB now that the Emgu dependency is gone.
- The `.iss` needs a check for the installed .NET Desktop Runtime 10 and, if missing, a download step
  (Inno Setup 6.3 can do this through `DownloadTemporaryFile`).
- In exchange the runtime stays current through Windows Update.

For a small background utility, variant A is the more pragmatic choice, especially since today's setup
already carries a payload of 55 MB.

Further changes to the distribution:

- `Setup\build-setup-files.bat`: `dotnet publish` will need `-r win-x64` and, depending on the variant,
  `--self-contained true|false`.
- `Setup\CameraActivityChecker-Setup.iss`: bump the version, for variant A also add
  `ArchitecturesInstallIn64BitMode=x64compatible`, for variant B add the runtime check.
- The `.iss` is currently not UTF-8 encoded, an editor shows `H?mmer Electronics` there. Inno Setup 6
  is Unicode only, so the file should be saved as UTF-8 with BOM while we are at it.
- AppVeyor: the build image has to provide the .NET 10 SDK. Since there is no `appveyor.yml` in the
  repository, that has to be done on the AppVeyor website. Checking in an `appveyor.yml` would make
  sense anyway, then the CI state is versioned.

## What is explicitly not a problem

- **resx files.** The `mimetype="application/x-microsoft.net.object.binary.base64"` hits in `Main.resx`
  and `Notification.resx` sit inside the template comment block, not in real entries. The actual
  resources (`$this.Icon`, `$this.BackgroundImage`) go through type converters, not through the
  BinaryFormatter. So the BinaryFormatter removed in .NET 9 is not an issue here, which the green build
  confirms.
- **P/Invoke.** `GetForegroundWindow`, `SetForegroundWindow`, `AnimateWindow` and `CreateRoundRectRgn`
  work unchanged on .NET 10.
- **WinForms designer files.** No third party control in the designer, `Main.Designer.cs` and
  `Notification.designer.cs` only contain built-in types.
- **`.editorconfig`.** The rules (file scoped namespaces, usings inside the namespace, `this.`
  qualification) keep applying unchanged.
- **GitVersion 5.11.1.** Works, see above.

Side note on the build environment: this machine has a Telerik feed in a global NuGet configuration
that answers with 404 and aborts the restore. That is not a project problem, a project-local
`NuGet.config` with `<clear />` and nuget.org avoids it reliably.

## Open points for the implementation

1. ~~**Smoke test with a real camera.**~~ Done, and it changed the plan, see "Camera detection: what
   the smoke test showed".
2. ~~**Licensing.**~~ Settled by dropping the dependency. Emgu CV is dual licensed under GPL v3 in 3.1
   as well as in 4.13, while the project itself is MIT. Without Emgu CV the question no longer arises.
3. **Still open: there is no way to quit the application** other than the task manager. That was
   already the case before, but with a working message loop a `NotifyIcon` with a context menu is now
   a small addition. It needs one more entry in both language files.
4. **Still open: the notification window is named** `EDGE Shop Flag Notification` internally, a
   leftover from wherever the class was taken from.

## Recommendations beyond the migration itself

These points were not part of the move to .NET 10 when this was written. Both of them were implemented
in the end, see "What was implemented in the end".

**1. The application never reaches its message loop.**
`Application.Run(new Main())` never gets its turn, because the constructor of `Main` runs into a
`while (true)` loop through `CheckCameraActivated()` and never returns. So the notifications are shown
without a running message pump, with a `Thread.Sleep(2000)` on the UI thread in between. A
`System.Windows.Forms.Timer` or a background task plus `NotifyIcon` would be the clean solution. Fitting
that picture, every iteration creates a new `VideoCapture` and cleans up after it with `GC.Collect()`
plus `GC.WaitForPendingFinalizers()`.

**2. Camera detection could work without EmguCV.**
Right now the camera is opened in order to find out whether the camera is open. Windows has been
keeping track of this itself since Windows 10, under
`HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam`
(plus `\NonPackaged` for classic programs). Every subkey has a `LastUsedTimeStop`, where the value `0`
means "is accessing right now". On this machine the key exists and is populated, so the approach works.
That would mean:

- no EmguCV, no 130 MB, no GPL question,
- no need to switch on your own camera just to check it,
- as a bonus the information **which** program is using the camera.

This is a bigger change than the migration and should be kept separate from it.

## Suggested order

1. ~~Switch to `net10.0-windows`, update SDK and packages, change `Capture` to `VideoCapture`.~~ Done.
2. ~~Work through the six nullability and obsolete spots, the build has to be green with
   `TreatWarningsAsErrors`.~~ Done.
3. ~~Remove `System.Resources.Extensions` and `OpenTK.dll.config`.~~ Done.
4. ~~Smoke test with a real camera, switching it on and off.~~ Done, and it led to dropping Emgu.CV
   in favour of the registry based detection.
5. **Open:** decide on the publish variant (A or B), adjust `build-setup-files.bat` and the `.iss`,
   build the setup and test it on a machine without .NET 10.
6. **Open:** move AppVeyor to the .NET 10 SDK, ideally with a checked-in `appveyor.yml`.
7. ~~Update `Changelog.md` and `README.md`, set the version to 1.1.0.0.~~ Done.

## Effort

| Step | Effort |
| --- | --- |
| Steps 1 to 3 (code and project file) | around 1 hour, already played through in the proof of concept |
| Step 4 (camera test) | 15 minutes, may cause rework if Emgu 4 reacts differently |
| Step 5 (setup) | 1 to 3 hours, depending on the variant |
| Steps 6 and 7 | 30 minutes |
