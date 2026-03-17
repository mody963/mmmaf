using System;
using System.IO;
using System.Media;

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
            using var player = new SoundPlayer(path);
            player.PlaySync();
        }
        catch
        {
            // ignore errors playing sound so it doesn't crash the app
        }
    }
}
