namespace CameraActivityChecker.Notifications;

using System;

/// <summary>
///     The directions in which the Roll and Slide animations can be shown
/// </summary>
/// <remarks>
///     Horizontal and vertical directions can be combined to create diagonal animations
/// </remarks>
[Flags]
public enum AnimationDirection
{
    /// <summary>
    ///     From left to right
    /// </summary>
    Right = 0x1,

    /// <summary>
    ///     From right to left
    /// </summary>
    Left = 0x2,

    /// <summary>
    ///     From top to bottom
    /// </summary>
    Down = 0x4,

    /// <summary>
    ///     From bottom to top
    /// </summary>
    Up = 0x8
}