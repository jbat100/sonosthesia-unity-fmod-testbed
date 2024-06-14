using System;
using FMOD.Studio;
using UniRx;
using UnityEngine;

namespace Sonosthesia
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(FMODEventInstance), true)]
    public class FMODEventInstanceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FMODEventInstance instance = (FMODEventInstance)target;
            if(GUILayout.Button("Restart"))
            {
                instance.Restart();
            }
            if(GUILayout.Button("Stop"))
            {
                instance.Stop();
            }
        }
    }
#endif
    
    public abstract class FMODEventInstance : MonoBehaviour
    {
        private readonly BehaviorSubject<EventInstance> _instancesSubject = new (default);

        public IObservable<EventInstance> InstanceObservable => _instancesSubject.AsObservable();

        public EventInstance EventInstance
        {
            get => _instancesSubject.Value;
            protected set => _instancesSubject.OnNext(value);
        }

        public abstract void Restart();
        
        public abstract void Stop();
    }
}