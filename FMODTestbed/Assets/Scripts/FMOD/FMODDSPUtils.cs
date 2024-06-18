using FMOD;
using FMOD.Studio;
using FMODUnity;

namespace Sonosthesia
{
    public static class FMODDSPUtils
    {
        private const int MAX_NAME_LENGTH = 100;
        
        public static RESULT CreateSideChainChannelGroup(string suffix, EventInstance instance, out ChannelGroup sidechain)
        {
            RESULT result;
            sidechain = default;
            
            result = instance.getChannelGroup(out ChannelGroup instanceChannelGroup); 
            UnityEngine.Debug.LogWarning($"{nameof(CreateSideChainChannelGroup)} getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            result = instanceChannelGroup.getName(out string instanceChannelGroupName, MAX_NAME_LENGTH);
            UnityEngine.Debug.LogWarning($"{nameof(CreateSideChainChannelGroup)} getName {result}");
            if (result != RESULT.OK)
            {
                return result;
            }
            
            string name = instanceChannelGroupName + suffix; 

            return CreateSideChainChannelGroup(name, instanceChannelGroup, out sidechain);

        }
        
        // TODO : this does not create a side chain, the output of the created group is the master group channel
        // so the filtered signals are actually played
        
        public static RESULT CreateSideChainChannelGroup(string name, ChannelGroup input, out ChannelGroup sidechain)
        {
            RESULT result;
            
            result = RuntimeManager.CoreSystem.createChannelGroup(name, out sidechain);
            UnityEngine.Debug.LogWarning($"{nameof(CreateSideChainChannelGroup)} createChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return result;
            }
            
            result = sidechain.addGroup(input); 
            UnityEngine.Debug.LogWarning($"{nameof(CreateSideChainChannelGroup)} addGroup {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }

        public static RESULT InsertDSP(ChannelGroup group, DSP_TYPE dspType, int index, out DSP dsp)
        {
            RESULT result;
            
            result = RuntimeManager.CoreSystem.createDSPByType(dspType, out dsp);
            UnityEngine.Debug.LogWarning($"{nameof(InsertDSP)} createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            result = group.addDSP(index, dsp);
            UnityEngine.Debug.LogWarning($"{nameof(InsertDSP)} addDSP {result}");
            if (result != RESULT.OK)
            {
                return result;
            }

            return RESULT.OK;
        }
    }
}