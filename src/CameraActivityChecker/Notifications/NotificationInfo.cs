namespace CameraActivityChecker.Notifications;

using System.Drawing;

/// <summary>
/// The notification information.
/// </summary>
public class NotificationInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationInfo"/> class.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="roInfo">The RO information.</param>
    /// <param name="updated">The updated text.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="color">The color.</param>
    /// <param name="duration">The duration.</param>
    public NotificationInfo(string title, string roInfo, string updated, Image icon, Color color, int duration)
    {
        this.Title = title;
        this.RoInfo = roInfo;
        this.Updated = updated;
        this.Icon = icon;
        this.Color = color;
        this.Duration = duration;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationInfo"/> class.
    /// </summary>
    public NotificationInfo()
    {
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the RO information.
    /// </summary>
    public string RoInfo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated text.
    /// </summary>
    public string Updated { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon.
    /// </summary>
    public Image? Icon { get; set; }

    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    public Color Color { get; set; } = Color.Empty;

    /// <summary>
    /// Gets or sets the duration.
    /// </summary>
    public int Duration { get; set; }
}
