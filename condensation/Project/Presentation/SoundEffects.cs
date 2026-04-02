using Project.Services;

public static class SoundEffects
{
    private static IUiSoundPlayer _player = new NoOpUiSoundPlayer();

    public static void Configure(IUiSoundPlayer player)
    {
        _player = player ?? new NoOpUiSoundPlayer();
    }

    public static void PlayMenuClick() => _player.PlayMenuClick();

    public static void PlayErrorSound() => _player.PlayErrorSound();

    public static void PlayKaching() => _player.PlayKaching();

    private sealed class NoOpUiSoundPlayer : IUiSoundPlayer
    {
        public void PlayMenuClick()
        {
        }

        public void PlayErrorSound()
        {
        }

        public void PlayKaching()
        {
        }
    }
}
