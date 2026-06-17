using Xunit;
using Project.Services;
using System.IO;


public class UiSoundSystemTests
{

    [Fact]
    public void UiSoundPlayer_SoundFilesExist()
    {
        string baseDir = Directory.GetCurrentDirectory();
        var player = new UiSoundPlayer(baseDir);

        Assert.True(File.Exists(Path.Combine(baseDir, "Sounds", "sound-4.wav")));
        Assert.True(File.Exists(Path.Combine(baseDir, "Sounds", "universfield-error-08-206492.wav")));
        Assert.True(File.Exists(Path.Combine(baseDir, "Sounds", "Cash Register (Kaching) - Sound Effect (HD) - Gaming Sound FX (youtube).wav")));
    }

    [Fact]
    public void UiSoundPlayer_PlayMenuClick_DoesNotThrow()
    {
        string baseDir = Directory.GetCurrentDirectory();
        var player = new UiSoundPlayer(baseDir);

        var ex = Record.Exception(() => player.PlayMenuClick());

        Assert.Null(ex);
    }
    [Fact]
    public void UiSoundPlayer_PlayErrorSound_DoesNotCrash_OnAnyOS()
    {
        string baseDir = Directory.GetCurrentDirectory();
        var player = new UiSoundPlayer(baseDir);

        var ex = Record.Exception(() => player.PlayErrorSound());

        Assert.Null(ex);
    }
    [Fact]
    public void UiSoundPlayer_MissingFile_DoesNotThrow()
    {
        string baseDir = Directory.GetCurrentDirectory();

        // Fake directory without sounds
        string fakeDir = Path.Combine(baseDir, "FakeSounds");

        var player = new UiSoundPlayer(fakeDir);

        var ex = Record.Exception(() => player.PlayKaching());

        Assert.Null(ex);
    }
}