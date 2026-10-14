using Glimpse.API;

namespace SimpleDelayEffect;

public record struct DelayConfig : IConfig
{
    public const string ConfigName = "SimpleDelayConfig";
    
    public float DelaySeconds;

    public float DecayFactor;

    public DelayConfig(float delaySeconds, float decayFactor)
    {
        DelaySeconds = delaySeconds;
        DecayFactor = decayFactor;
    }
}