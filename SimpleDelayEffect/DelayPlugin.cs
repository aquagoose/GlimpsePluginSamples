using System.Runtime.InteropServices;
using Glimpse.API;
using Glimpse.API.Codecs;
using Glimpse.API.UI;

namespace SimpleDelayEffect;

public class DelayPlugin : IPlugin
{
    public const float MaxDelayTime = 1f;
    
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
        _buffer = new float[(uint) (44100 * 2 * MaxDelayTime)];
        _numSamples = (uint) (44100 * 2 * _config.DelaySeconds);
        _glimpse.Player.AddEffect(_effect);
    }

    public void OnGUI(IImmediateGUI ui)
    {
        if (ui.Slider("Delay Time", ref _workingConfig.DelaySeconds, 0.001f, MaxDelayTime))
        {
            _numSamples = (uint) (44100 * 2 * _workingConfig.DelaySeconds);
            _currentIndex = 0;
        }
        ui.Slider("Decay Factor", ref _workingConfig.DecayFactor, 0.001f, 1.0f);
    }

    private void DelayEffect(Span<byte> buffer, AudioFormat format)
    {
        Span<short> data = MemoryMarshal.Cast<byte, short>(buffer);

        for (int i = 0; i < data.Length; i++)
        {
            float fData = data[i] / (float) short.MaxValue;
            
            float oldSample = _buffer[_currentIndex];
            float sample = fData + (oldSample * _workingConfig.DecayFactor);
            _buffer[_currentIndex] = sample;
            _currentIndex = (_currentIndex + 1) % _numSamples;

            data[i] = (short) (float.Clamp(sample, -1, 1) * short.MaxValue);
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