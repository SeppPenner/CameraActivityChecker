# Project rules for Claude

## What this is

CameraActivityChecker is a Windows only background application (`WinExe`, `net10.0-windows`) that
tells the user when a camera is switched on or off. There is no window and no tray icon, the
application talks to the user through its own notification popups only.

- Solution `src/CameraActivityChecker.sln` with the single project
  `src/CameraActivityChecker/CameraActivityChecker.csproj`. There are no tests.
- `CameraUsageDetector` reads the capability access data that Windows maintains itself
  (`SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam`, in
  `HKCU` and in `HKLM`, including the `NonPackaged` subkey). A `LastUsedTimeStop` value of zero
  means that the program owning the subkey is using the camera right now. Nothing in the
  application ever opens the camera itself.
- `Main` polls the detector with a `System.Windows.Forms.Timer` once per second and shows a
  notification whenever the state changes.
- `Notifications/` holds the notification form, the `FormAnimator` and the P/Invoke wrappers in
  `NativeMethods`.
- Texts come from `languages/de-DE.xml` and `languages/en-US.xml` through the NuGet package
  `HaemmerElectronics.SeppPenner.Language`. Both files are copied next to the executable.
- `analyse.md` documents the migration from .NET Framework 4.8.1 to .NET 10 and the reason why the
  EmguCV dependency was dropped. It is kept as written, the result was appended to it.

## Build

- `dotnet build src/CameraActivityChecker.sln -c Release`
- `TreatWarningsAsErrors` is enabled and `NuGetAudit` runs in `all` mode, so a build that produces
  warnings is a failed build. `NU1803` is suppressed because of the additional package sources on
  this machine.
- The content of the installer is produced by `Setup/build-setup-files.bat`: it deletes every `bin`
  and `obj` below `src`, publishes self contained for `win-x64` into
  `src/CameraActivityChecker/bin/publish` and removes the `*.pdb` files afterwards.
- The installer itself is compiled by Inno Setup 6 from `Setup/CameraActivityChecker-Setup.iss`,
  the batch file does not do that.

## Code conventions

`src/.editorconfig` is the authority:

- CRLF, 4 spaces, UTF-8 without BOM.
- File scoped namespaces, using directives inside the namespace, System directives first.
- `this.` qualification for fields, properties, methods and events.
- `IDE0005` (unnecessary using) is a warning, and warnings are errors.
- Every type and member carries an XML documentation comment. Comments are English.
- The language XML files use tabs for indentation and have no trailing newline. Keep that when
  editing them, and keep them UTF-8 without BOM.

## Known quirks

- The main form is never shown, `SetVisibleCore` always passes `false` and `ShowInTaskbar` is off.
  There is no tray icon and no menu, so a running instance can only be stopped through the task
  manager.
- The state at startup is the reference. A camera that is already in use when the application
  starts produces no notification, only a change from that point on does.
- `GetNotification` passes the same text as title and as body on purpose, the notification form
  shows both labels.
- The rounded corners of the notification are set in the `Shown` handler through `SetWindowRgn`.
  Setting the region earlier does not work, the animation of `FormAnimator` drives the window
  region itself and the notification ends up clipped.
- `GetWord` returns `null` for an unknown key and does not fall back to another language. A new
  key has to be added to every language file.
- `Webcam.ico` exists twice, as `src/Webcam.ico` and as `src/CameraActivityChecker/Webcam.ico`.
  Only the one inside the project folder is used, by `ApplicationIcon` and by the setup.
- The built installer `Setup/CameraActivityChecker-Setup.exe` is tracked in the repository, the
  `.gitignore` does not exclude it. Every release adds its full size to the history permanently.
- The README shows an AppVeyor badge, but there is no build configuration in the repository.
- `PrivilegesRequired` is not set while the quick launch icon points into `{userappdata}`. Inno
  Setup would warn about that, the task is limited to Windows 7 and older through
  `OnlyBelowVersion: 0,6.1` and therefore never runs.

## Releasing

The order matters, the version is produced by GitVersion out of the tags:

1. Add the entry to `Changelog.md` (version with four parts, date).
2. Set `MyAppVersion` in `Setup/CameraActivityChecker-Setup.iss` (keep UTF-8 with BOM and CRLF).
3. Commit the changes.
4. Set the lightweight tag on that commit, for example `git tag 1.1.1`. All existing tags are
   lightweight. The tag has to exist **before** the publish, otherwise GitVersion burns a
   prerelease version into the shipped executable.
5. Run `Setup/build-setup-files.bat`.
6. Compile `Setup/CameraActivityChecker-Setup.iss` with `ISCC.exe`.
7. Commit the new installer with the message `Updated setup.`.
8. Push the branch and the tag.

## Git

- Never use `git commit --amend`, not even for a commit that only exists locally. Add a follow up
  commit instead. The versions of this project come from GitVersion out of tags that sit on exact
  commits, so rewriting a commit moves the ground under a tag.

## Commits

- Commit messages are written **in English only**.
- Short, precise summary in the subject line, plus an explanatory body when needed.

## Punctuation

- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments
  nor documentation.
- Use a regular hyphen, comma, colon, parentheses or a separate sentence instead.

## Code comments

- Comments in code (and in project files such as `.csproj`) are **always written in English**,
  regardless of the language used in the rest of the communication.

## German texts

- In German texts (documentation, chat replies) always use **real umlauts and ß**, never ASCII
  transliterations.
- Rewrite where needed:
  - `ae` -> `ä`
  - `oe` -> `ö`
  - `ue` -> `ü`
  - `Ae` -> `Ä`, `Oe` -> `Ö`, `Ue` -> `Ü`
  - `ss` -> `ß` (only where orthographically correct, e.g. `Strasse` -> `Straße`; `dass` stays
    `dass`)
- This applies to documentation files and chat, **not** to code comments (those are English,
  see above).
- Exception: identifiers, file names, configuration keys and similar stay unchanged when umlauts
  are technically undesirable there.
