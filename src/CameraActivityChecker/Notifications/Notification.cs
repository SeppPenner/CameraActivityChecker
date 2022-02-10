namespace CameraActivityChecker.Notifications;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// The notification form.
/// </summary>
public partial class Notification : Form
{
    /// <summary>
    /// The open notifications.
    /// </summary>
    private static readonly List<Notification> OpenNotifications = new ();

    /// <summary>
    /// The animator form.
    /// </summary>
    private readonly FormAnimator animator;

    /// <summary>
    /// A value indicating whether the focus is allowed or not.
    /// </summary>
    private bool allowFocus;

    /// <summary>
    /// The current foreground window.
    /// </summary>
    private IntPtr currentForegroundWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="Notification"/> class.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="body">The body.</param>
    /// <param name="duration">The duration.</param>
    /// <param name="animation">The animation.</param>
    /// <param name="direction">The direction.</param>
    public Notification(string title, string body, int duration, AnimationMethod animation,
        AnimationDirection direction)
    {
        this.InitializeComponent();

        if (duration < 0)
        {
            duration = int.MaxValue;
        }
        else
        {
            duration = duration * 1000;
        }

        this.lifeTimer.Interval = duration;
        this.labelTitle.Text = title;
        this.labelBody.Text = body;

        this.animator = new FormAnimator(this, animation, direction, 500);

        this.Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, this.Width - 5, this.Height - 5, 20, 20));
    }

    /// <summary>
    ///     Displays the form.
    /// </summary>
    /// <remarks>
    ///     Required to allow the form to determine the current foreground window before being displayed.
    /// </remarks>
    public new void Show()
    {
        // Determine the current foreground window so it can be reactivated each time this form tries to get the focus
        this.currentForegroundWindow = NativeMethods.GetForegroundWindow();

        base.Show();
    }

    /// <summary>
    /// The notification load handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NotificationLoad(object sender, EventArgs e)
    {
        // Display the form just above the system tray.
        this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width,
            Screen.PrimaryScreen.WorkingArea.Height - this.Height);

        // Move each open form upwards to make room for this one
        foreach (var openForm in OpenNotifications)
        {
            openForm.Top -= this.Height;
        }

        OpenNotifications.Add(this);
        this.lifeTimer.Start();
    }

    /// <summary>
    /// The notification activated handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NotificationActivated(object sender, EventArgs e)
    {
        // Prevent the form taking focus when it is initially shown
        if (!this.allowFocus)
        {
            NativeMethods.SetForegroundWindow(this.currentForegroundWindow);
        }
    }

    /// <summary>
    /// The notification shown handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NotificationShown(object sender, EventArgs e)
    {
        // Once the animation has completed the form can receive focus
        this.allowFocus = true;

        // Close the form by sliding down.
        this.animator.Duration = 0;
        this.animator.Direction = AnimationDirection.Down;
    }

    /// <summary>
    /// The notification form closed handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NotificationFormClosed(object sender, FormClosedEventArgs e)
    {
        // Move down any open forms above this one
        foreach (var openForm in OpenNotifications)
        {
            if (openForm == this)
            {
                break;
            }

            openForm.Top += this.Height;
        }

        OpenNotifications.Remove(this);
    }

    /// <summary>
    /// The life timer tick handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void LifeTimerTick(object sender, EventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// The notification click handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void NotificationClick(object sender, EventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// The title label click handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void LabelTitleClick(object sender, EventArgs e)
    {
        this.Close();
    }

    /// <summary>
    /// The RO label click handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private void LabelROClick(object sender, EventArgs e)
    {
        this.Close();
    }
}
