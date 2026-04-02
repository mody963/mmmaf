using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;

namespace Project.Services
{
    public class UiSoundPlayer : IUiSoundPlayer
    {
        private readonly string _clickSoundPath;
        private readonly string _errorSoundPath;
        private readonly string _kachingSoundPath;

        public UiSoundPlayer(string baseDirectory)
        {
            _clickSoundPath = Path.Combine(baseDirectory, "Sounds", "sound-4.wav");
            _errorSoundPath = Path.Combine(baseDirectory, "Sounds", "universfield-error-08-206492.wav");
            _kachingSoundPath = Path.Combine(baseDirectory, "Sounds", "Cash Register (Kaching) - Sound Effect (HD) - Gaming Sound FX (youtube).wav");
        }

        public void PlayMenuClick()
        {
            Play(_clickSoundPath);
        }

        public void PlayErrorSound()
        {
            Play(_errorSoundPath);
        }

        public void PlayKaching()
        {
            Play(_kachingSoundPath);
        }

        private static void Play(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var player = new SoundPlayer(filePath);
                    player.Play();
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    return;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "paplay",
                        Arguments = $"\"{filePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch
            {
                // Sound playback should never crash the app.
            }
        }
    }
}
