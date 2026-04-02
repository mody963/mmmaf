using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class SoundEffects
{
    private static readonly string MenuClickSoundPath =
        Path.Combine(AppContext.BaseDirectory, "Sounds", "sound-4.wav");

    private static readonly string KachingSoundPath =
        Path.Combine(AppContext.BaseDirectory, "Sounds", "Cash Register (Kaching) - Sound Effect (HD) - Gaming Sound FX (youtube).wav");

    private static readonly string ErrorSoundPath =
        Path.Combine(AppContext.BaseDirectory, "Sounds", "universfield-error-08-206492.wav");

    public static void PlayMenuClick()
    {
        PlaySound(MenuClickSoundPath);
    }

    public static void PlayKaching()
    {
        PlaySound(KachingSoundPath);
    }

    public static void PlayErrorSound()
    {
        PlaySound(ErrorSoundPath);
    }

    private static void PlaySound(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Use PowerShell's SoundPlayer on Windows.
                var windowsCommand =
                    "Add-Type -AssemblyName System.Media; " +
                    "$player = New-Object System.Media.SoundPlayer('" + path.Replace("'", "''") + "'); " +
                    "$player.PlaySync()";

                RunProcess("powershell", "-NoProfile", "-Command", windowsCommand);
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                RunProcess("afplay", path);
                return;
            }

            // Linux fallback: try common audio players in order.
            if (TryRunProcess("paplay", path))
            {
                return;
            }

            TryRunProcess("aplay", path);
        }
        catch
        {
            // ignore errors playing sound so it doesn't crash the app
        }
    }

    private static void RunProcess(string fileName, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
        process.WaitForExit();
    }

    private static bool TryRunProcess(string fileName, params string[] arguments)
    {
        try
        {
            RunProcess(fileName, arguments);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
