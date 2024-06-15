using FMOD;

namespace Sonosthesia
{
    public enum LoudnessSelector
    {
        None,
        Momentary,
        ShortTerm,
        Integrated,
        Percentile10,
        Percentile95,
        MaxTruePeak,
        MaxMomentary
    }

    public static class LoudnessSelectionExtensions
    {
        public static float Select(this ref DSP_LOUDNESS_METER_INFO_TYPE info, LoudnessSelector selector)
        {
            return selector switch
            {
                LoudnessSelector.Integrated => info.integratedloudness,
                LoudnessSelector.Momentary => info.momentaryloudness,
                LoudnessSelector.ShortTerm => info.shorttermloudness,
                LoudnessSelector.Percentile10 => info.loudness10thpercentile,
                LoudnessSelector.Percentile95 => info.loudness95thpercentile,
                LoudnessSelector.MaxTruePeak => info.maxtruepeak,
                LoudnessSelector.MaxMomentary => info.maxtruepeak,
                _ => 0
            };
        }
    }
}