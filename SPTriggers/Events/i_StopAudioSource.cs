﻿#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;

using com.spacepuppy.Tween;

namespace com.spacepuppy.Events
{

    public class i_StopAudioSource : AutoTriggerable
    {

        #region Fields

        [SerializeField]
        [TriggerableTargetObject.Config(typeof(AudioSource))]
        private TriggerableTargetObject _target;

        [SerializeField]
        [TimeUnitsSelector()]
        private float _fadeOutDur;

        #endregion


        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            var targ = _target.GetTarget<AudioSource>(arg);
            if (targ == null) return false;
            if (!targ.isPlaying) return false;

            if (_fadeOutDur > 0f)
            {
                float cache = targ.volume;
                SPTween.Tween(targ)
                       .To("volume", 0f, _fadeOutDur)
                       .OnFinish((s, e) =>
                       {
                           targ.Stop();
                           targ.volume = cache;
                       })
                       .Play(true);
            }
            else
            {
                targ.Stop();
            }

            return true;
        }
    }

}
