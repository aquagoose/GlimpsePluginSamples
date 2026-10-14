using Glimpse.API;
using Glimpse.API.Library;

namespace MyFirstGlimpsePlugin;

public class MyPlugin : IPlugin
{
    private IGlimpse _glimpse = null!;
    
    public bool IsInitialized { get; private set; }

    public bool ConfigChanged => false;
    
    public void Initialize(IGlimpse glimpse)
    {
        _glimpse = glimpse;
        IsInitialized = true;
        
        // add a button to the top row, which plays a random song from the user's library when clicked.
        // every button must have a unique ID, an icon, and an optional tooltip.
        // using asset:// allows you to load an icon directly from glimpse itself, in this case we are
        // loading the shuffle icon.
        _glimpse.AddButton("RandomSong", "asset://Icons.Shuffle.png", "Play a random song!", PlayRandomSong);
    }

    private void PlayRandomSong()
    {
        // get all the tracks in the user's library.
        SizedCollection<Track> tracks = _glimpse.Library.GetTracks();
        // pick a random track from the list
        int value = Random.Shared.Next((int) tracks.Count);
        Track trackToPlay = tracks.Collection.ElementAt(value);
        // queue the track, clearing the current play queue
        _glimpse.Player.QueueTrack(trackToPlay.Path, QueueSlot.Clear);
        // play the queue, at index 0, which will play our track.
        _glimpse.Player.Play(0);
    }
    
    public void Dispose()
    {
        // we must remember to remove buttons when we are done with them!
        // Dispose can be called at any time (including when a plugin is disabled), so we must clean up
        // everything we don't need.
        _glimpse.RemoveButton("RandomSong");
    }
}
