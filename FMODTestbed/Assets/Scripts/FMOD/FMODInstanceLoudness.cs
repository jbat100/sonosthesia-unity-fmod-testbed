using System;
using FMOD.Studio;
using UnityEngine;
using Sonosthesia.Signal;
using UniRx;

namespace Sonosthesia
{
    public abstract class FMODInstanceLoudness : FMODInstanceProcessor
    {
        [SerializeField] private LoudnessSelector _selector = LoudnessSelector.Momentary;
        
        [SerializeField] private Signal<float> _target;
        

        protected override void Process()
        {
            if (_target && TryGetLoudness(_selector, out float loudness))
            {
                _target.Broadcast(loudness);
            }
        }

        protected abstract bool TryGetLoudness(LoudnessSelector selector, out float loudness);

    }
}