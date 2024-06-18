using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace Sonosthesia
{
    public class FMODInstanceLoudnessChained : FMODInstanceLoudness
    {
        private ChannelGroup _instanceChannelGroup;
        private DSP _meterDSP;
        
        protected override void Cleanup()
        {
            if (_instanceChannelGroup.hasHandle() && _meterDSP.hasHandle())
            {
                // note : we don't own the instance channel group, it is not our business to release it
                _instanceChannelGroup.removeDSP(_meterDSP);
                _instanceChannelGroup = default;
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
            
            result = RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.LOUDNESS_METER, out _meterDSP);
            UnityEngine.Debug.LogWarning($"createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = instance.getChannelGroup(out _instanceChannelGroup); 
            UnityEngine.Debug.LogWarning($"getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = _instanceChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, _meterDSP);
            UnityEngine.Debug.LogWarning($"addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }

        protected override bool TryGetLoudness(LoudnessSelector selector, out float loudness)
        {
            return LoudnessSelectionExtensions.GetLoudness(_meterDSP, selector, out loudness);
        }
    }    
}


