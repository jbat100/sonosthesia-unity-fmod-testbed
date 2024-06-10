using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace Sonosthesia
{
    public class TrackVolumeUI : MonoBehaviour
    {
        private const string BASS_VOLUME_PARAMETER = "BassVolume";
        private const string DRUMS_VOLUME_PARAMETER = "DrumsVolume";

        [SerializeField] private StudioEventEmitter _emitter;
        
        [SerializeField] private Slider _bassSlider;
        [SerializeField] private Slider _drumsSlider;

        protected virtual void Start()
        {
            //_emitter.EventDescription.loadSampleData();

            Setup(_bassSlider, BASS_VOLUME_PARAMETER);
            Setup(_drumsSlider, DRUMS_VOLUME_PARAMETER);
        }

        private void Setup(Slider slider, string parameter)
        {
            _emitter.EventInstance.getParameterByName(parameter, out float value);
            slider.value = value;
            slider.onValueChanged.AddListener(val => _emitter.SetParameter(parameter, val));
        }
    }    
}


