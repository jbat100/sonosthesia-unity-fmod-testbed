using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODEventVolume : MonoBehaviour
    {
        [SerializeField] private EventReference _fmodEventPath;

        [SerializeField, Range(0, 1)] private float _volume;
        
        private EventInstance eventInstance;

        protected virtual void Start()
        {
            // Create an instance of the event
            eventInstance = RuntimeManager.CreateInstance(_fmodEventPath);
            
            eventInstance.getVolume(out _volume);
            
            // Start the event
            eventInstance.start();
        }

        protected virtual void Update()
        {
            eventInstance.setVolume(_volume);
        }

        void OnDestroy()
        {
            // Stop and release the event instance when the object is destroyed
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }    
}


