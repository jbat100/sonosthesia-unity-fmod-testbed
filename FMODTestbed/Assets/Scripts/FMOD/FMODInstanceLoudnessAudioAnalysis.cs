using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sonosthesia.Pack;
using Sonosthesia.Signal;
using UnityEngine;

namespace Sonosthesia
{
    public enum AudioAnalysisBand
    {
        None,
        Lows,
        Mids,
        Highs
    }
    
    internal abstract class BandLoudnessAnalysis : IDisposable
    {
        private ChannelGroup _dspChannelGroup;
        private DSP _filterDSP;
        private DSP _loudnessDSP;

        protected DSP FilterDSP => _filterDSP;

        protected abstract RESULT CreateFilter(ChannelGroup dspGroup, out DSP filterDSP);

        public bool GetLoudness(LoudnessSelector selector, out float loudness)
        {
            return LoudnessSelectionExtensions.GetLoudness(_loudnessDSP, selector, out loudness);
        }
        
        public bool TrySetup(EventInstance instance)
        {
            if (!instance.isValid())
            {
                UnityEngine.Debug.LogWarning($"Setup called with invalid handle");
                return false;
            }
            
            RESULT result;

            result = FMODDSPUtils.CreateSideChainChannelGroup(" Audio Analysis", instance, out _dspChannelGroup);
            if (result != RESULT.OK)
            {
                return false;
            }

            result = CreateFilter(_dspChannelGroup, out _filterDSP);
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = _dspChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, _filterDSP);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = _filterDSP.setActive(true);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} setActive {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = FMODDSPUtils.InsertDSP(_dspChannelGroup, DSP_TYPE.LOUDNESS_METER, CHANNELCONTROL_DSP_INDEX.TAIL, out _loudnessDSP);
            if (result != RESULT.OK)
            {
                return false;
            }

            result = _loudnessDSP.setActive(true);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} setActive {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }
        
        public void Dispose()
        {
            if (_dspChannelGroup.hasHandle())
            {
                if (_filterDSP.hasHandle())
                {
                    _dspChannelGroup.removeDSP(_filterDSP);    
                }
                
                if (_loudnessDSP.hasHandle())
                {
                    _dspChannelGroup.removeDSP(_loudnessDSP);    
                }
                
                _dspChannelGroup.release();
                _dspChannelGroup = default;
            }

            if (_filterDSP.hasHandle())
            {
                _filterDSP.release();
                _filterDSP = default;
            }
            
            if (_loudnessDSP.hasHandle())
            {
                _loudnessDSP.release();
                _loudnessDSP = default;
            }
        }
    }

    internal abstract class SimpleBandLoudnessAnalysis : BandLoudnessAnalysis
    {
        private float _cutoff;
        public float Cutoff
        {
            get => _cutoff;
            set
            {
                if (Math.Abs(value - _cutoff) > 1e-3)
                {
                    _cutoff = value;
                    ApplyCutoff();
                }
            }
        }

        public SimpleBandLoudnessAnalysis(float cutoff)
        {
            _cutoff = cutoff;
        }

        protected abstract DSP_TYPE DSPType { get; }
        protected abstract int CutoffParameterIndex { get; }
        
        protected override RESULT CreateFilter(ChannelGroup dspGroup, out DSP filterDSP)
        {
            RESULT result = RuntimeManager.CoreSystem.createDSPByType(DSPType, out filterDSP);
            UnityEngine.Debug.LogWarning($"{this} createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return ApplyCutoff();
        }

        private RESULT ApplyCutoff()
        {
            DSP dsp = FilterDSP;

            RESULT result = dsp.setParameterFloat(CutoffParameterIndex, _cutoff);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }

    }
    
    internal class SimpleLowBandLoudnessAnalysis : SimpleBandLoudnessAnalysis
    {
        public SimpleLowBandLoudnessAnalysis(float cutoff = 500f) : base(cutoff)
        {
        }

        protected override DSP_TYPE DSPType => DSP_TYPE.LOWPASS_SIMPLE;
        protected override int CutoffParameterIndex => (int)DSP_LOWPASS_SIMPLE.CUTOFF;
    }
    
    internal class SimpleHighBandLoudnessAnalysis : SimpleBandLoudnessAnalysis
    {
        public SimpleHighBandLoudnessAnalysis(float cutoff = 5000f) : base(cutoff)
        {
        }
        
        protected override DSP_TYPE DSPType => DSP_TYPE.HIGHPASS_SIMPLE;
        protected override int CutoffParameterIndex => (int)DSP_HIGHPASS_SIMPLE.CUTOFF;
    }

    internal class EQBandLoudnessAnalysis : BandLoudnessAnalysis
    {
        private float PASS_GAIN = 0;
        private float BLOCK_GAIN = -80;
        
        private readonly AudioAnalysisBand _band;
        private readonly DSP_THREE_EQ_CROSSOVERSLOPE_TYPE _crossoverSlope;

        public EQBandLoudnessAnalysis(AudioAnalysisBand band, DSP_THREE_EQ_CROSSOVERSLOPE_TYPE crossoverSlope, float lowCrossover, float highCrossover)
        {
            _band = band;
            _crossoverSlope = crossoverSlope;
            _lowCrossover = lowCrossover;
            _highCrossover = highCrossover;
        }
        
        private float _lowCrossover = 500;
        public float LowCrossover
        {
            get => _lowCrossover;
            set
            {
                if (Math.Abs(value - _lowCrossover) > 1e-3)
                {
                    _lowCrossover = value;
                    ApplyLowCrossover(FilterDSP);
                }
            }
        }
        
        private float _highCrossover = 5000;
        public float HighCrossover
        {
            get => _highCrossover;
            set
            {
                if (Math.Abs(value - _highCrossover) > 1e-3)
                {
                    _highCrossover = value;
                    ApplyHighCrossover(FilterDSP);
                }
            }
        }

        private RESULT ApplyLowCrossover(DSP dsp)
        {
            RESULT result = dsp.setParameterFloat((int)DSP_THREE_EQ.LOWCROSSOVER, _lowCrossover);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.LOWCROSSOVER} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }
        
        private RESULT ApplyHighCrossover(DSP dsp)
        {
            RESULT result = dsp.setParameterFloat((int)DSP_THREE_EQ.HIGHCROSSOVER, _highCrossover);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.HIGHCROSSOVER} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }
        
        protected override RESULT CreateFilter(ChannelGroup dspGroup, out DSP filterDSP)
        {
            RESULT result = RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.THREE_EQ, out filterDSP);
            UnityEngine.Debug.LogWarning($"{this} createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return result;
            }
            
            if (ApplyLowCrossover(filterDSP) != RESULT.OK)
            {
                return result;
            }
            
            if (ApplyHighCrossover(filterDSP) != RESULT.OK)
            {
                return result;
            }
            
            result = filterDSP.setParameterInt((int)DSP_THREE_EQ.CROSSOVERSLOPE, (int)_crossoverSlope);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.CROSSOVERSLOPE} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            float lowGain = _band == AudioAnalysisBand.Lows ? PASS_GAIN : BLOCK_GAIN;
            float midGain = _band == AudioAnalysisBand.Mids ? PASS_GAIN : BLOCK_GAIN;
            float highGain = _band == AudioAnalysisBand.Highs ? PASS_GAIN : BLOCK_GAIN;

            result = filterDSP.setParameterFloat((int)DSP_THREE_EQ.LOWGAIN, lowGain);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.LOWGAIN} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }
            
            result = filterDSP.setParameterFloat((int)DSP_THREE_EQ.MIDGAIN, midGain);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.MIDGAIN} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }
            
            result = filterDSP.setParameterFloat((int)DSP_THREE_EQ.HIGHGAIN, highGain);
            UnityEngine.Debug.LogWarning($"{this} setParameterFloat {DSP_THREE_EQ.HIGHGAIN} {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }
    }
    
    public class FMODInstanceLoudnessAudioAnalysis : FMODInstanceProcessor
    {
        [SerializeField] private Signal<AudioAnalysis> _target;

        [SerializeField] private LoudnessSelector _selector = LoudnessSelector.Momentary;

        [SerializeField] private DSP_THREE_EQ_CROSSOVERSLOPE_TYPE _crossoverSlope = DSP_THREE_EQ_CROSSOVERSLOPE_TYPE._12DB;
        
        [SerializeField] private float _lowCrossover = 500;

        [SerializeField] private float _highCrossover = 5000;

        private float _startTime;
        private BandLoudnessAnalysis _lowBandAnalysis;
        private BandLoudnessAnalysis _midBandAnalysis;
        private BandLoudnessAnalysis _highBandAnalysis;
        
        protected override bool TrySetup(EventInstance instance)
        {
            _startTime = Time.time;
            
            bool success = true;
            
            // Despite creating a new group the filter dsp is heard in the rendered audio
            // look into connection types:
            // https://www.fmod.com/docs/2.02/api/core-api-dspconnection.html#fmod_dspconnection_type

            _lowBandAnalysis = new EQBandLoudnessAnalysis(AudioAnalysisBand.Lows, _crossoverSlope, _lowCrossover, _highCrossover);
            success &= _lowBandAnalysis.TrySetup(instance);
            
            _midBandAnalysis = new EQBandLoudnessAnalysis(AudioAnalysisBand.Mids, _crossoverSlope, _lowCrossover, _highCrossover);
            success &= _midBandAnalysis.TrySetup(instance);
            
            _highBandAnalysis = new EQBandLoudnessAnalysis(AudioAnalysisBand.Highs, _crossoverSlope, _lowCrossover, _highCrossover);
            success &= _lowBandAnalysis.TrySetup(instance);

            if (!success)
            {
                Cleanup();
            }
            
            return success;

        }

        protected override void Cleanup()
        {
            _lowBandAnalysis?.Dispose();
            _lowBandAnalysis = null;
            
            _midBandAnalysis?.Dispose();
            _midBandAnalysis = null;
            
            _highBandAnalysis?.Dispose();
            _highBandAnalysis = null;
        }

        protected override void Process()
        {
            if (!_target)
            {
                return;
            }
            
            float lows = 0, mids = 0, highs = 0;

            _lowBandAnalysis?.GetLoudness(_selector, out lows);
            _midBandAnalysis?.GetLoudness(_selector, out mids);
            _highBandAnalysis?.GetLoudness(_selector, out highs);

            float time = Time.time - _startTime;

            AudioAnalysis analysis = new AudioAnalysis
            {
                time = time,
                lows = lows,
                mids = mids,
                highs = highs
            };
            
            _target.Broadcast(analysis);
        }
    }
}