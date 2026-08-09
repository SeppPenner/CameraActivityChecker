namespace CameraActivityChecker.Notifications;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// A class of native Windows methods.
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    ///     Gets the handle of the window that currently has focus.
    /// </summary>
    /// <returns>
    ///     The handle of the window that currently has focus.
    /// </returns>
    [DllImport("user32")]
    internal static extern IntPtr GetForegroundWindow();

    /// <summary>
    ///     Activates the specified window.
    /// </summary>
    /// <param name="hWnd">
    ///     The handle of the window to be focused.
    /// </param>
    /// <returns>
    ///     True if the window was focused; False otherwise.
    /// </returns>
    [DllImport("user32")]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    ///     Windows API function to animate a window.
    /// </summary>
    [DllImport("user32")]
    internal static extern bool AnimateWindow(IntPtr hWnd, int dwTime, int dwFlags);

    /// <summary>
    ///     Sets the region of a window, everything outside of it is not drawn.
    /// </summary>
    /// <param name="hWnd">The handle of the window.</param>
    /// <param name="hRgn">The region. The window takes ownership of it, so it must not be deleted afterwards.</param>
    /// <param name="bRedraw">A value indicating whether the window is redrawn or not.</param>
    /// <returns>Zero if the call failed.</returns>
    [DllImport("user32")]
    internal static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

    /// <summary>
    /// Creates a round rectangle region.
    /// </summary>
    /// <param name="nLeftRect">The X-coordinate of the upper-left corner value.</param>
    /// <param name="nTopRect">The Y-coordinate of the upper-left corner value.</param>
    /// <param name="nRightRect">The X-coordinate of the lower-right corner value.</param>
    /// <param name="nBottomRect">The Y-coordinate of the lower-right corner value.</param>
    /// <param name="nWidthEllipse">The width of the ellipse.</param>
    /// <param name="nHeightEllipse">The height of the ellipse.</param>
    /// <returns>The handle of the created region.</returns>
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    internal static extern IntPtr CreateRoundRectRgn(
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse);
}
