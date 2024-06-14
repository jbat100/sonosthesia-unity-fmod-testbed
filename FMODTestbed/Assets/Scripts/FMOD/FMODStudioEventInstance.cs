using FMODUnity;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODStudioEventInstance : FMODEventInstance
    {
        [SerializeField] private StudioEventEmitter _emitter;

        public override void Restart()
        {
            _emitter.Play();
        }

        public override void Stop()
        {
            _emitter.Stop();
        }
        
        protected virtual void Update()
        {
            // A bit nasty but StudioEventEmitter has no way of knowing from outside 
            // if a new event instance has been created for sure, and previous one
            // shot may be still playing
            
            if (_emitter.EventInstance.handle != EventInstance.handle)
            {
                EventInstance = _emitter.EventInstance;
            }
        }
    }
}