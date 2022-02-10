namespace CameraActivityChecker;

using System;
using System.Windows.Forms;

/// <summary>
/// The main program.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Main());
    }
}
