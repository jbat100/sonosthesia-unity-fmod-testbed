using System;
using FMOD.Studio;
using UniRx;
using UnityEngine;

namespace Sonosthesia
{
    public abstract class FMODInstanceProcessor : MonoBehaviour
    {
        [SerializeField] private FMODInstance _instance;
        
        private IDisposable _subscription;
        private EventInstance _currentInstance;

        public bool SetupDone { get; private set; }

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
                SetupDone = false;
            });
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
            Cleanup();
        }
        
        protected virtual void Update()
        {
            if (_currentInstance.isValid() && !SetupDone)
            {
                SetupDone = TrySetup(_currentInstance);
            }
            
            if (!SetupDone)
            {
                return;
            }
            
            Process();
        }
        
        protected abstract bool TrySetup(EventInstance instance);
        
        protected abstract void Cleanup();
        
        /// <summary>
        /// Called on each Update if Processor is properly set up
        /// </summary>
        protected virtual void Process()
        {
            
        }
    }
}