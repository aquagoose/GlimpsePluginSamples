using Glimpse.API;
using Glimpse.API.Library;

namespace MyFirstGlimpsePlugin;

public class MyPlugin : IPlugin
{
    private IGlimpse _glimpse;
    
    public bool IsInitialized { get; private set; }
    
    public bool ConfigChanged { get; private set; }
    
    public void Initialize(IGlimpse glimpse)
    {
        _glimpse = glimpse;
        IsInitialized = true;
        
        _glimpse.AddButton("RandomSong", "asset://Icons.Shuffle.png", "Play random song", () =>
        {
            SizedCollection<Track> tracks = _glimpse.Library.GetTracks();
            int value = Random.Shared.Next((int) tracks.Count);
            Track trackToPlay = tracks.Collection.ElementAt(value);
            _glimpse.Player.QueueTrack(trackToPlay.Path, QueueSlot.Clear);
            _glimpse.Player.Play(0);
        });
    }
    
    public void Dispose()
    {
        _glimpse.RemoveButton("RandomSong");
    }
}
