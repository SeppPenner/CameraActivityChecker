namespace CameraActivityChecker.Notifications;

using System;
using System.ComponentModel;
using System.Windows.Forms;

/// <summary>
///     Animates a form when it is shown, hidden or closed.
/// </summary>
/// <remarks>
///     MDI child forms do not support the Fade method and only support other methods while being displayed for the first
///     time and when closing
/// </remarks>
public sealed class FormAnimator
{
    /// <summary>
    ///     Hide the form
    /// </summary>
    private const int AwHide = 0x10000;

    /// <summary>
    ///     Activate the form
    /// </summary>
    private const int AwActivate = 0x20000;

    /// <summary>
    ///     The number of milliseconds over which the animation occurs if no value is specified
    /// </summary>
    private const int DefaultDuration = 250;

    /// <summary>
    ///     The form to be animated
    /// </summary>
    private readonly Form form;

    /// <summary>
    ///     The direction in which to Roll or Slide the form
    /// </summary>
    private AnimationDirection direction;

    /// <summary>
    ///     The number of milliseconds over which the animation is played
    /// </summary>
    private int duration;

    /// <summary>
    ///     The animation method used to show and hide the form
    /// </summary>
    private AnimationMethod method;

    /// <summary>
    ///     Creates a new <b>FormAnimator</b> object for the specified form
    /// </summary>
    /// <param name="form">
    ///     The form to be animated
    /// </param>
    /// <remarks>
    ///     No animation will be used unless the <b>Method</b> and/or <b>Direction</b> properties are set independently. The
    ///     <b>Duration</b> is set to quarter of a second by default.
    /// </remarks>
    public FormAnimator(Form form)
    {
        this.form = form;

        this.form.Load += this.Form_Load;
        this.form.VisibleChanged += this.Form_VisibleChanged;
        this.form.Closing += this.Form_Closing;

        this.duration = DefaultDuration;
    }

    /// <summary>
    ///     Creates a new <b>FormAnimator</b> object for the specified form using the specified method over the specified
    ///     duration
    /// </summary>
    /// <param name="form">
    ///     The form to be animated
    /// </param>
    /// <param name="method">
    ///     The animation method used to show and hide the form
    /// </param>
    /// <param name="duration">
    ///     The number of milliseconds over which the animation is played
    /// </param>
    /// <remarks>
    ///     No animation will be used for the <b>Roll</b> or <b>Slide</b> methods unless the <b>Direction</b> property is set
    ///     independently
    /// </remarks>
    public FormAnimator(Form form, AnimationMethod method, int duration) : this(form)
    {
        this.method = method;
        this.duration = duration;
    }

    /// <summary>
    ///     Creates a new <b>FormAnimator</b> object for the specified form using the specified method in the specified
    ///     direction over the specified duration
    /// </summary>
    /// <param name="form">
    ///     The form to be animated
    /// </param>
    /// <param name="method">
    ///     The animation method used to show and hide the form
    /// </param>
    /// <param name="direction">
    ///     The direction in which to animate the form
    /// </param>
    /// <param name="duration">
    ///     The number of milliseconds over which the animation is played
    /// </param>
    /// <remarks>
    ///     The <i>direction</i> argument will have no effect if the <b>Center</b> or <b>Fade</b> method is
    ///     specified
    /// </remarks>
    public FormAnimator(Form form, AnimationMethod method, AnimationDirection direction, int duration) : this(form,
        method, duration)
    {
        this.direction = direction;
    }

    /// <summary>
    ///     Gets or sets the animation method used to show and hide the form
    /// </summary>
    /// <value>
    ///     The animation method used to show and hide the form
    /// </value>
    /// <remarks>
    ///     <b>Roll</b> is used by default if no method is specified
    /// </remarks>
    public AnimationMethod Method
    {
        get => this.method;
        set => this.method = value;
    }

    /// <summary>
    ///     Gets or Sets the direction in which the animation is performed
    /// </summary>
    /// <value>
    ///     The direction in which the animation is performed
    /// </value>
    /// <remarks>
    ///     The direction is only applicable to the <b>Roll</b> and <b>Slide</b> methods
    /// </remarks>
    public AnimationDirection Direction
    {
        get => this.direction;
        set => this.direction = value;
    }

    /// <summary>
    ///     Gets or Sets the number of milliseconds over which the animation is played
    /// </summary>
    /// <value>
    ///     The number of milliseconds over which the animation is played
    /// </value>
    public int Duration
    {
        get => this.duration;
        set => this.duration = value;
    }

    /// <summary>
    ///     Gets the form to be animated
    /// </summary>
    /// <value>
    ///     The form to be animated
    /// </value>
    public Form Form => this.form;

    /// <summary>
    ///     Animates the form automatically when it is loaded
    /// </summary>
    private void Form_Load(object sender, EventArgs e)
    {
        // MDI child forms do not support transparency so do not try to use the Fade method
        if (this.form.MdiParent is null || this.method != AnimationMethod.Fade)
        {
            NativeMethods.AnimateWindow(this.form.Handle, this.duration, AwActivate | (int)this.method | (int)this.direction);
        }
    }

    /// <summary>
    ///     Animates the form automatically when it is shown or hidden
    /// </summary>
    private void Form_VisibleChanged(object sender, EventArgs e)
    {
        // Do not attempt to animate MDI child forms while showing or hiding as they do not behave as expected
        if (this.form.MdiParent is null)
        {
            var flags = (int)this.method | (int)this.direction;

            if (this.form.Visible)
            {
                flags |= AwActivate;
            }
            else
            {
                flags |= AwHide;
            }

            NativeMethods.AnimateWindow(this.form.Handle, this.duration, flags);
        }
    }

    /// <summary>
    ///     Animates the form automatically when it closes
    /// </summary>
    private void Form_Closing(object sender, CancelEventArgs e)
    {
        if (e.Cancel)
        {
            return;
        }
        // MDI child forms do not support transparency so do not try to use the Fade method.
        if (this.form.MdiParent is null || this.method != AnimationMethod.Fade)
        {
            NativeMethods.AnimateWindow(this.form.Handle, this.duration, AwHide | (int)this.method | (int)this.direction);
        }
    }
}
