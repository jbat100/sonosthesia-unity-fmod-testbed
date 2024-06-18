using System;
using FMOD.Studio;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Sonosthesia
{
    public class TrackVolumeUI : MonoBehaviour
    {
        private const string BASS_VOLUME_PARAMETER = "BassVolume";
        private const string DRUMS_VOLUME_PARAMETER = "DrumsVolume";

        [SerializeField] private FMODInstance _instance;
        
        [SerializeField] private Slider _bassSlider;
        [SerializeField] private Slider _drumsSlider;

        private IDisposable _instanceSubscription;
        private readonly CompositeDisposable _sliderSubscriptions = new();

        protected virtual void OnEnable()
        {
            _instanceSubscription = _instance.InstanceObservable.Subscribe(i =>
            {
                _sliderSubscriptions.Clear();
                
                IDisposable Setup(Slider slider, EventInstance instance, string parameter)
                {
                    instance.getParameterByName(parameter, out float parameterValue);
                    slider.value = parameterValue;
                    return slider.onValueChanged.AsObservable().Subscribe(sliderValue =>
                    {
                        instance.setParameterByName(parameter, sliderValue);
                    });
                }
                
                _sliderSubscriptions.Add(Setup(_bassSlider, i, BASS_VOLUME_PARAMETER));
                _sliderSubscriptions.Add(Setup(_drumsSlider, i, DRUMS_VOLUME_PARAMETER));
            });
        }

        protected virtual void OnDisable()
        {
            _instanceSubscription?.Dispose();
            _sliderSubscriptions.Clear();
        }
    }    
}


