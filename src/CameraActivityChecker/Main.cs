namespace CameraActivityChecker;

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CameraActivityChecker.Notifications;
using Languages.Implementation;
using Languages.Interfaces;

/// <summary>
/// The main form.
/// </summary>
public partial class Main : Form
{
    /// <summary>
    /// The interval in milliseconds between two checks.
    /// </summary>
    private const int CheckIntervalInMilliseconds = 1000;

    /// <summary>
    /// The number of seconds a notification stays on screen.
    /// </summary>
    private const int NotificationDurationInSeconds = 3;

    /// <summary>
    /// The identifier of the language that is used if none of the language files fits the user interface language.
    /// </summary>
    private const string FallbackLanguageIdentifier = "de-DE";

    /// <summary>
    /// The timer that triggers the checks.
    /// </summary>
    private readonly System.Windows.Forms.Timer checkTimer = new();

    /// <summary>
    /// A value indicating whether the camera is activated or not.
    /// </summary>
    private bool cameraActivated;

    /// <summary>
    /// The language.
    /// </summary>
    /// <remarks>
    /// The language manager itself is deliberately not kept in a field. Creating it reads the language files, and a
    /// field initializer runs before the constructor body, so a broken installation would take the application down
    /// before <see cref="TryInitialize"/> could report it.
    /// </remarks>
    private ILanguage? language;

    /// <summary>
    /// Initializes a new instance of the <see cref="Main"/> class.
    /// </summary>
    public Main()
    {
        this.InitializeComponent();
        this.TryInitialize();

        // The state at startup is the reference, only a change from here on is worth a notification.
        this.cameraActivated = CameraUsageDetector.IsCameraInUse();

        this.checkTimer.Interval = CheckIntervalInMilliseconds;
        this.checkTimer.Tick += this.CheckTimerTick;
        this.checkTimer.Start();
    }

    /// <summary>
    /// Keeps the main form hidden, the application only works through its notifications.
    /// </summary>
    /// <param name="value">A value indicating whether the form should be visible or not.</param>
    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    /// <summary>
    /// Tries to initialize the component.
    /// </summary>
    private void TryInitialize()
    {
        try
        {
            this.Initialize();
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    /// <summary>
    /// Initializes the component.
    /// </summary>
    private void Initialize()
    {
        ILanguageManager languageManager = new LanguageManager();
        languageManager.SetCurrentLanguage(GetLanguageIdentifier(languageManager));
        this.language = languageManager.GetCurrentLanguage();
    }

    /// <summary>
    /// Gets the identifier of the language that fits the user interface language of Windows.
    /// </summary>
    /// <param name="languageManager">The language manager holding the loaded languages.</param>
    /// <returns>The identifier of the language to use.</returns>
    /// <remarks>
    /// Setting a language that is not loaded throws, so an identifier that is really there is preferred over anything
    /// else. An exact match wins, then a language file for the same two letter language, then German, then whatever
    /// was loaded first.
    /// </remarks>
    private static string GetLanguageIdentifier(ILanguageManager languageManager)
    {
        var identifiers = languageManager.GetLanguages().Select(loadedLanguage => loadedLanguage.Identifier).ToList();
        var culture = CultureInfo.CurrentUICulture;

        return identifiers.Find(identifier => identifier.Equals(culture.Name, StringComparison.OrdinalIgnoreCase))
            ?? identifiers.Find(identifier => identifier.StartsWith($"{culture.TwoLetterISOLanguageName}-", StringComparison.OrdinalIgnoreCase))
            ?? identifiers.Find(identifier => identifier.Equals(FallbackLanguageIdentifier, StringComparison.OrdinalIgnoreCase))
            ?? identifiers.FirstOrDefault()
            ?? FallbackLanguageIdentifier;
    }

    /// <summary>
    /// Shows an error to the user.
    /// </summary>
    /// <param name="ex">The exception that was caught.</param>
    /// <remarks>
    /// The title is the product name and not a translated text on purpose: the only way into this method is a failure
    /// while the language files are being loaded, so there is no language left to take a title from. Without this
    /// dialog the application either dies without a word or keeps running without ever showing a notification, the
    /// console output it wrote instead goes nowhere in a Windows application.
    /// </remarks>
    private static void ShowError(Exception ex)
    {
        var text = $"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}";
        MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// The check timer tick handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void CheckTimerTick(object? sender, EventArgs e)
    {
        this.CheckCameraIsActive();
    }

    /// <summary>
    /// Checks whether the camera is activated.
    /// </summary>
    private void CheckCameraIsActive()
    {
        try
        {
            var activated = CameraUsageDetector.IsCameraInUse();

            if (this.cameraActivated == activated)
            {
                return;
            }

            this.cameraActivated = activated;
            this.ShowNotification(activated);
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Sends a notification.
    /// </summary>
    /// <param name="activated">A value indicating whether the camera is activated or not.</param>
    private void ShowNotification(bool activated)
    {
        if (activated)
        {
            this.ShowNotificationCameraActivated();
        }
        else
        {
            this.ShowNotificationCameraDeactivated();
        }
    }

    /// <summary>
    /// Shows a notification that the camera is deactivated.
    /// </summary>
    private void ShowNotificationCameraDeactivated()
    {
        if (this.language is null)
        {
            throw new ArgumentNullException(nameof(this.language), "The language wasn't set properly.");
        }

        var message = this.language.GetWord("CameraDeactivated") ?? string.Empty;
        this.GetNotification(message).Show();
    }

    /// <summary>
    /// Shows a notification that the camera is activated.
    /// </summary>
    private void ShowNotificationCameraActivated()
    {
        if (this.language is null)
        {
            throw new ArgumentNullException(nameof(this.language), "The language wasn't set properly.");
        }

        var message = this.language.GetWord("CameraActivated") ?? string.Empty;
        this.GetNotification(message).Show();
    }

    /// <summary>
    /// Gets a notification.
    /// </summary>
    /// <param name="message">The messsage.</param>
    /// <returns>A <see cref="Notification"/>.</returns>
    private Notification GetNotification(string message)
    {
        // The notification closes itself through its own life timer.
        return new Notification(
            message,
            message,
            NotificationDurationInSeconds,
            AnimationMethod.Center,
            AnimationDirection.Down);
    }
}
