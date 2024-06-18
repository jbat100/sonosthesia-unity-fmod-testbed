using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODFFT : FMODProcessor
    {
        [SerializeField] private DSP_FFT_WINDOW _windowType;

        [SerializeField] private int numberOfSamples = 1024;

        public int NumberOfSamples => numberOfSamples;
        
        private DSP _fftDSP;
        
        public bool GetSpectrumData(float[] spectrum, int channel)
        {
            if (!IsSetup)
            {
                return false;
            }

            RESULT result = _fftDSP.getParameterData((int)DSP_FFT.SPECTRUMDATA, out IntPtr unmanagedData, out uint length);
            if (result != RESULT.OK)
            {
                UnityEngine.Debug.LogWarning($"_fftDSP getParameterData {result}");
                return false;
            }
                
            DSP_PARAMETER_FFT fftData = (DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(DSP_PARAMETER_FFT));
            if (fftData.numchannels <= channel)
            {
                UnityEngine.Debug.LogWarning($"fftData numchannels {fftData.numchannels}");
                return false;
            }

            fftData.getSpectrum(channel, ref spectrum);
            
            return true;
        }
        
        protected override bool PerformTrySetup(ChannelGroup channelGroup)
        {
            RESULT result = RuntimeManager.CoreSystem.createDSPByType(DSP_TYPE.FFT, out _fftDSP);
            UnityEngine.Debug.LogWarning($"createDSPByType {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            result = _fftDSP.setParameterInt((int)DSP_FFT.WINDOWTYPE, (int)_windowType);
            UnityEngine.Debug.LogWarning($"setParameterInt {DSP_FFT.WINDOWTYPE} {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = _fftDSP.setParameterInt((int)DSP_FFT.WINDOWSIZE, numberOfSamples * 2);
            UnityEngine.Debug.LogWarning($"setParameterInt {DSP_FFT.WINDOWSIZE} {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            RuntimeManager.StudioSystem.flushCommands();
            
            result = channelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, _fftDSP);
            UnityEngine.Debug.LogWarning($"addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }

        protected override void PerformCleanup(ChannelGroup channelGroup)
        {
            if (channelGroup.hasHandle() && _fftDSP.hasHandle())
            {
                // note : we don't own the instance channel group, it is not our business to release it
                channelGroup.removeDSP(_fftDSP);
            }
            
            if (_fftDSP.hasHandle())
            {
                _fftDSP.release();
                _fftDSP = default;
            }
        }
    }
}
