using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace Sonosthesia
{
    public class FMODInstanceLoudnessSidechained : FMODInstanceLoudness
    {
        private ChannelGroup _originalChannelGroup;
        private ChannelGroup _dspChannelGroup;
        private DSP _meterDSP;

        protected override void Cleanup()
        {
            if (_dspChannelGroup.hasHandle() && _meterDSP.hasHandle())
            {
                _dspChannelGroup.removeDSP(_meterDSP);
            }

            if (_dspChannelGroup.hasHandle())
            {
                _dspChannelGroup.release();
                _dspChannelGroup = default;
            }

            if (_meterDSP.hasHandle())
            {
                _meterDSP.release();
                _meterDSP = default;
            }
        }
        
        protected override bool TrySetup(EventInstance instance)
        {
            if (!instance.isValid())
            {
                UnityEngine.Debug.LogWarning($"Setup called with invalid handle");
                return false;
            }
            
            RESULT result;
            
            result = RuntimeManager.CoreSystem.createChannelGroup("Parrallel DSP", out _dspChannelGroup);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} createChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.LOUDNESS_METER, out _meterDSP);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = instance.getChannelGroup(out _originalChannelGroup); 
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = _dspChannelGroup.addGroup(_originalChannelGroup); 
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = _dspChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, _meterDSP);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            //result = originalChannelGroup.getDSP(CHANNELCONTROL_DSP_INDEX.TAIL, out DSP channelGroupDSP);
            //UnityEngine.Debug.LogWarning($"getDSP {result}");
            //if (result != RESULT.OK)
            //{
            //    return false;
            //}

            //result = channelGroupDSP.getType(out DSP_TYPE dspType);
            //UnityEngine.Debug.LogWarning($"getType {dspType}");
            //if (result != RESULT.OK)
            //{
            //    return false;
            //}
            
            result = _originalChannelGroup.getNumDSPs(out int numDSPs);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} getNumDSPs {numDSPs}");
            if (result != RESULT.OK)
            {
                return false;
            }

            //result = meterDSP.addInput(channelGroupDSP);
            //UnityEngine.Debug.LogWarning($"addInput {result}");
            //if (result != RESULT.OK)
            //{
            //    return false;
            //}
            
            result = _meterDSP.setActive(true);
            UnityEngine.Debug.LogWarning($"{nameof(TrySetup)} setActive {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }

        protected override bool TryGetLoudness(LoudnessSelector selector, out float loudness)
        {
            if (!_meterDSP.hasHandle())
            {
                loudness = 0;
                return false;
            }
            
            // Get the metering data from the DSP meter
            _meterDSP.getParameterData((int)DSP_LOUDNESS_METER.INFO, out IntPtr data, out uint length);

            // https://www.fmod.com/docs/2.01/api/core-api-common-dsp-effects.html#fmod_dsp_loudness_meter_info_type
            DSP_LOUDNESS_METER_INFO_TYPE info =
                (DSP_LOUDNESS_METER_INFO_TYPE)Marshal.PtrToStructure(data, typeof(DSP_LOUDNESS_METER_INFO_TYPE));

            loudness = info.Select(selector);
            return true;
        }
    }
}