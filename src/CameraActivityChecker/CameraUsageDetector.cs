namespace CameraActivityChecker;

using System;
using Microsoft.Win32;

/// <summary>
/// Detects whether the camera is currently in use by reading the capability access data that Windows maintains itself.
/// </summary>
/// <remarks>
/// Windows keeps one subkey per program below the consent store. As long as a program is using the camera, its
/// <c>LastUsedTimeStop</c> value is zero, afterwards it holds the file time of the moment the access ended. Reading this
/// data does not touch the camera at all, so the camera is neither blocked for other programs nor switched on.
/// </remarks>
public static class CameraUsageDetector
{
    /// <summary>
    /// The consent store key for the camera.
    /// </summary>
    private const string ConsentStoreKeyPath =
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam";

    /// <summary>
    /// The subkey that holds the classic (not packaged) programs.
    /// </summary>
    private const string NonPackagedKeyName = "NonPackaged";

    /// <summary>
    /// The value that is zero while a program is using the camera.
    /// </summary>
    private const string LastUsedTimeStopValueName = "LastUsedTimeStop";

    /// <summary>
    /// Gets a value indicating whether any program is currently using the camera.
    /// </summary>
    /// <returns>True if at least one program is using the camera, false if not.</returns>
    public static bool IsCameraInUse()
    {
        // Packaged apps are tracked per user, classic programs can also show up below the machine hive.
        return IsCameraInUse(Registry.CurrentUser) || IsCameraInUse(Registry.LocalMachine);
    }

    /// <summary>
    /// Gets a value indicating whether any program below the given hive is currently using the camera.
    /// </summary>
    /// <param name="hive">The registry hive to look at.</param>
    /// <returns>True if at least one program is using the camera, false if not.</returns>
    private static bool IsCameraInUse(RegistryKey hive)
    {
        try
        {
            using var consentStore = hive.OpenSubKey(ConsentStoreKeyPath);

            if (consentStore is null)
            {
                return false;
            }

            if (ContainsProgramUsingCamera(consentStore))
            {
                return true;
            }

            using var nonPackaged = consentStore.OpenSubKey(NonPackagedKeyName);
            return nonPackaged is not null && ContainsProgramUsingCamera(nonPackaged);
        }
        catch (Exception)
        {
            // A hive that cannot be read must not take the application down.
            return false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether one of the direct subkeys marks a running camera access.
    /// </summary>
    /// <param name="parent">The key whose subkeys are checked.</param>
    /// <returns>True if at least one subkey marks a running camera access, false if not.</returns>
    private static bool ContainsProgramUsingCamera(RegistryKey parent)
    {
        foreach (var subKeyName in parent.GetSubKeyNames())
        {
            if (subKeyName == NonPackagedKeyName)
            {
                continue;
            }

            try
            {
                using var subKey = parent.OpenSubKey(subKeyName);

                if (subKey?.GetValue(LastUsedTimeStopValueName) is long lastUsedTimeStop && lastUsedTimeStop == 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // A single unreadable program entry must not hide the other ones.
            }
        }

        return false;
    }
}
