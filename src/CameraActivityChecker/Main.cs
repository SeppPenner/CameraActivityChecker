namespace CameraActivityChecker;

using System;
using System.Threading;
using System.Windows.Forms;
using CameraActivityChecker.Notifications;
using Emgu.CV;
using Languages.Implementation;
using Languages.Interfaces;

/// <summary>
/// The main form.
/// </summary>
public partial class Main : Form
{
    /// <summary>
    /// A value indicating whether the camera is activated or not.
    /// </summary>
    private bool cameraActivated;

    /// <summary>
    /// The capture.
    /// </summary>
    private Capture capture = new Capture();

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
        this.CheckCameraActivated();
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
    /// Checks whether the camera is activated.
    /// </summary>
    private void CheckCameraActivated()
    {
        while (true)
        {
            this.CheckCameraIsActive();
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
    /// Checks whether the camera is activated.
    /// </summary>
    private void CheckCameraIsActive()
    {
        try
        {
            var activated = this.IsCameraActivated();
            GC.Collect();
            GC.WaitForPendingFinalizers();

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
        var toastNotification = this.GetNotification(message);
        this.ShowNotificationDelayed(toastNotification);
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
        var toastNotification = this.GetNotification(message);
        this.ShowNotificationDelayed(toastNotification);
    }

    /// <summary>
    /// Shows the notification delayed.
    /// </summary>
    /// <param name="toastNotification">The notification.</param>
    private void ShowNotificationDelayed(Notification toastNotification)
    {
        toastNotification.Show();
        Thread.Sleep(2000);
        toastNotification.Hide();
    }

    /// <summary>
    /// Gets a notification.
    /// </summary>
    /// <param name="message">The messsage.</param>
    /// <returns>A <see cref="Notification"/>.</returns>
    private Notification GetNotification(string message)
    {
        return new Notification(message, message, 1000, AnimationMethod.Center, AnimationDirection.Down);
    }

    /// <summary>
    /// Gets a value indicating whether the camera is activated or not.
    /// </summary>
    /// <returns>True if the camera is activated, false if not.</returns>
    private bool IsCameraActivated()
    {
        this.capture = new Capture();
        var frame = this.capture.QueryFrame();
        return frame != null;
    }
}
