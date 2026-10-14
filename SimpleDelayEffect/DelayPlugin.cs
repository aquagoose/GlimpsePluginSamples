using Glimpse.API;
using Glimpse.API.Codecs;
using Glimpse.API.UI;

namespace SimpleDelayEffect;

public class DelayPlugin : IPlugin
{
    private const uint SampleRate = 48000; // glimpse uses 48khz sample rate. currently this is not adjustable
    private const uint Channels = 2; // glimpse provides a stereo output. currently this is not adjustable
    private const float MaxDelayTime = 1f;
    
    private IGlimpse _glimpse;
    private IAudioPlayer.AudioEffect _effect;
    private uint _numSamples;

    private DelayConfig _config;
    private DelayConfig _workingConfig;

    private float[] _buffer;
    private uint _currentIndex;

    public bool IsInitialized => true;

    public bool ConfigChanged => _workingConfig != _config;
    
    public void Initialize(IGlimpse glimpse)
    {
        _glimpse = glimpse;
        _effect = DelayEffect;

        if (!_glimpse.ConfigManager.TryGetConfig(DelayConfig.ConfigName, out _config))
        {
            _config = new DelayConfig(0.155f, 0.720f);
            _glimpse.ConfigManager.WriteConfig(DelayConfig.ConfigName, _config);
        }

        _workingConfig = _config;
        _buffer = new float[(uint) (SampleRate * Channels * MaxDelayTime)];
        _numSamples = (uint) (SampleRate * Channels * _config.DelaySeconds);
        _glimpse.Player.AddEffect(_effect);
    }

    public void OnGUI(IImmediateGUI ui)
    {
        if (ui.Slider("Delay Time", ref _workingConfig.DelaySeconds, 0.001f, MaxDelayTime))
        {
            _numSamples = (uint) (SampleRate * Channels * _workingConfig.DelaySeconds);
            _currentIndex = 0;
        }
        ui.Slider("Decay Factor", ref _workingConfig.DecayFactor, 0.001f, 1.0f);
    }

    private void DelayEffect(Span<float> buffer, in AudioFormat format)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            float oldSample = _buffer[_currentIndex];
            float sample = buffer[i] + (oldSample * _workingConfig.DecayFactor);
            _buffer[_currentIndex] = sample;
            _currentIndex = (_currentIndex + 1) % _numSamples;
            buffer[i] = sample;
        }
    }

    public void Dispose()
    {
        _glimpse.Player.RemoveEffect(_effect);
    }

    public void SaveConfigChanges()
    {
        _config = _workingConfig;
        _glimpse.ConfigManager.WriteConfig(DelayConfig.ConfigName, _config);
    }

    public void CancelConfigChanges()
    {
        _workingConfig = _config;
    }
}