using System;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sonosthesia.Audio;
using UniRx;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODInstanceSpectrum : AudioSpectrum
    {
        [SerializeField] private FMODEventInstance _instance;

        [SerializeField] private DSP_FFT_WINDOW _windowType;
        
        private DSP _fftDSP;
        private ChannelGroup _instanceChannelGroup;
        private EventInstance _currentInstance;
        private bool _setupDone;
        private float[] _spectrum;
        private IDisposable _subscription;

        protected virtual void OnDestroy()
        {
            Cleanup();
        }
        
        protected virtual void OnEnable()
        {
            _subscription = _instance.InstanceObservable.Subscribe(instance =>
            {
                Cleanup();
                _currentInstance = instance;
                _setupDone = false;
            });
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
        }
        
        protected override bool GetSpectrumData(float[] spectrum, int channel)
        {
            if (!_currentInstance.isValid())
            {
                return false;
            }
            
            if (!_setupDone)
            {
                _setupDone = TrySetup(_currentInstance);
            }

            if (!_setupDone)
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

        private void Cleanup()
        {
            if (_instanceChannelGroup.hasHandle() && _fftDSP.hasHandle())
            {
                // note : we don't own the instance channel group, it is not our business to release it
                _instanceChannelGroup.removeDSP(_fftDSP);
                _instanceChannelGroup = default;
            }
            
            if (_fftDSP.hasHandle())
            {
                _fftDSP.release();
                _fftDSP = default;
            }
        }

        private bool TrySetup(EventInstance instance)
        {
            if (!instance.isValid())
            {
                UnityEngine.Debug.LogWarning($"Setup called with invalid handle");
                return false;
            }

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
            
            result = _fftDSP.setParameterInt((int)DSP_FFT.WINDOWSIZE, NumberOfSamples * 2);
            UnityEngine.Debug.LogWarning($"setParameterInt {DSP_FFT.WINDOWSIZE} {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            RuntimeManager.StudioSystem.flushCommands();
            
            result = instance.getChannelGroup(out _instanceChannelGroup); 
            UnityEngine.Debug.LogWarning($"getChannelGroup {result}");
            if (result != RESULT.OK)
            {
                return false;
            }
            
            result = _instanceChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, _fftDSP);
            UnityEngine.Debug.LogWarning($"addDSP {result}");
            if (result != RESULT.OK)
            {
                return false;
            }

            return true;
        }
    }
    
}

