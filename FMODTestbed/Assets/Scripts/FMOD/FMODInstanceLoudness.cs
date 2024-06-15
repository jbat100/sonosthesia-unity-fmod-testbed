using System;
using FMOD.Studio;
using UnityEngine;
using Sonosthesia.Signal;
using UniRx;

namespace Sonosthesia
{
    public abstract class FMODInstanceLoudness : MonoBehaviour
    {
        [SerializeField] private FMODEventInstance _instance;

        [SerializeField] private LoudnessSelector _selector = LoudnessSelector.Momentary;
        
        [SerializeField] private Signal<float> _target;

        private IDisposable _subscription;
        private bool _setupDone;
        private EventInstance _currentInstance;
        
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
        
        protected virtual void Update()
        {
            if (_currentInstance.isValid() && !_setupDone)
            {
                _setupDone = TrySetup(_currentInstance);
            }
            
            if (!_setupDone)
            {
                return;
            }
            
            if (_target && TryGetLoudness(_selector, out float loudness))
            {
                _target.Broadcast(loudness);
            }
        }

        protected abstract void Cleanup();

        protected abstract bool TrySetup(EventInstance instance);

        protected abstract bool TryGetLoudness(LoudnessSelector selector, out float loudness);

    }
}