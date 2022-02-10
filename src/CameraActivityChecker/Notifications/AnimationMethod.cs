namespace CameraActivityChecker.Notifications;

/// <summary>
///     The methods of animation available.
/// </summary>
public enum AnimationMethod
{
    /// <summary>
    ///     Rolls out from edge when showing and into edge when hiding
    /// </summary>
    /// <remarks>
    ///     This is the default animation method and requires a direction
    /// </remarks>
    Roll = 0x0,

    /// <summary>
    ///     Expands out from center when showing and collapses into center when hiding
    /// </summary>
    Center = 0x10,

    /// <summary>
    ///     Slides out from edge when showing and slides into edge when hiding
    /// </summary>
    /// <remarks>
    ///     Requires a direction
    /// </remarks>
    Slide = 0x40000,

    /// <summary>
    ///     Fades from transaprent to opaque when showing and from opaque to transparent when hiding
    /// </summary>
    Fade = 0x80000
}