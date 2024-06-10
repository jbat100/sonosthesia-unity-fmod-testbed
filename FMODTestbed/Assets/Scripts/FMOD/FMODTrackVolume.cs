using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Sonosthesia
{
    public class FMODTrackVolume : MonoBehaviour
    {
        [FMODUnity.EventRef]
        public string fmodEventPath;
        
        private EventInstance eventInstance;

        protected virtual void Start()
        {
            // Create an instance of the event
            eventInstance = RuntimeManager.CreateInstance(fmodEventPath);
        
            // Start the event
            eventInstance.start();
        }

        public void SetTrackVolume(string parameterName, float volume)
        {
            // Get the parameter instance
            PARAMETER_ID parameterId;
            eventInstance.getDescription(out EventDescription eventDescription);
            eventDescription.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION parameterDescription);
            parameterId = parameterDescription.id;

            // Set the volume of the track
            eventInstance.setParameterByID(parameterId, volume);
        }

        void OnDestroy()
        {
            // Stop and release the event instance when the object is destroyed
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }    
}


