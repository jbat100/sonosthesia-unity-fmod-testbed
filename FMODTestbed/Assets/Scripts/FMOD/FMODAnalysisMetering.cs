using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UniRx;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODAnalysisMetering : MonoBehaviour
    {
        [SerializeField] private FMODEventInstance _instance;

        private IDisposable _subscription;
        
        private ChannelGroup instanceChannelGroup;
        private DSP meterDSP;

        private EventInstance currentInstance;
        private bool setupDone;

        protected virtual void OnEnable()
        {
            _subscription = _instance.InstanceObservable.Subscribe(instance =>
            {
                meterDSP.release();
                currentInstance = instance;
                setupDone = false;
            });
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
        }

        protected virtual bool TrySetup(EventInstance instance)
        {
            if (!instance.isValid())
            {
                UnityEngine.Debug.LogWarning($"Setup called with invalid handle");
                return false;
            }
            
            RESULT result;
            
            result = RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.LOUDNESS_METER, out meterDSP);
            UnityEngine.Debug.LogWarning($"createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = instance.getChannelGroup(out instanceChannelGroup); 
            UnityEngine.Debug.LogWarning($"getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = instanceChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, meterDSP);
            UnityEngine.Debug.LogWarning($"addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }

        protected virtual void Update()
        {
            if (currentInstance.isValid() && !setupDone)
            {
                setupDone = TrySetup(currentInstance);
            }
            
            if (!setupDone || !meterDSP.hasHandle())
            {
                return;
            }
            
            // Get the metering data from the DSP meter
            meterDSP.getParameterData((int)DSP_LOUDNESS_METER.INFO, out IntPtr data, out uint length);

            // https://www.fmod.com/docs/2.01/api/core-api-common-dsp-effects.html#fmod_dsp_loudness_meter_info_type
            DSP_LOUDNESS_METER_INFO_TYPE info =
                (DSP_LOUDNESS_METER_INFO_TYPE)Marshal.PtrToStructure(data, typeof(DSP_LOUDNESS_METER_INFO_TYPE));

            // Assuming the loudness meter returns an overall loudness value
            UnityEngine.Debug.Log($"Loudness: {info.momentaryloudness}");
        }
        
        protected virtual void OnDestroy()
        {
            instanceChannelGroup.removeDSP(meterDSP);
            // Release the event instance and DSP when the object is destroyed
            meterDSP.release();
        }
    }    
}


