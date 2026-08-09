namespace CameraActivityChecker;

using System;
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
    private ILanguage? language;

    /// <summary>
    /// The language manager.
    /// </summary>
    private ILanguageManager languageManager = new LanguageManager();

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
        catch
        {
            Console.WriteLine("");
        }
    }

    /// <summary>
    /// Initializes the component.
    /// </summary>
    private void Initialize()
    {
        this.languageManager = new LanguageManager();
        this.languageManager.SetCurrentLanguage("de-DE");
        this.language = this.languageManager.GetCurrentLanguage();
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
