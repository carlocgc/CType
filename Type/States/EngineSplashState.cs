﻿using AmosShared.Audio;
using AmosShared.State;
using System;
using Type.Scenes;
#if __ANDROID__
using Type.Android.States;
#endif

namespace Type.States
{
    /// <summary>
    /// Shows the Amos Engine splash screen
    /// </summary>
    public class EngineSplashState : State
    {
        /// <summary> The splash screen scene </summary>
        private EngineSplashScene _Scene;
        /// <summary> How long the scene has been active </summary>
        private TimeSpan _TimeSinceBegan;
        /// <summary> How long till the scene ends </summary>
        private TimeSpan _TimeTillEnd;
        /// <summary> The time to play the sound </summary>
        private TimeSpan _PlaySound;
        /// <summary> Whether the sound has played</summary>
        private Boolean _SoundPlayed;
        /// <summary> Whethe the state should end </summary>
        private Boolean _Complete;

        public EngineSplashState()
        {
        }

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _Scene = new EngineSplashScene();
            _TimeSinceBegan = TimeSpan.Zero;
            _TimeTillEnd = TimeSpan.FromSeconds(2.5);
            _PlaySound = TimeSpan.FromSeconds(0.5);
        }

        /// <inheritdoc />
        public override void Update(TimeSpan timeSinceUpdate)
        {
            if (!_Complete)
            {
                _TimeSinceBegan += timeSinceUpdate;
                if (_TimeSinceBegan > _PlaySound && !_SoundPlayed)
                {
                    new AudioPlayer("Content/Audio/Hello.wav", false, AudioManager.Category.EFFECT, 1);
                    _SoundPlayed = true;
                }

                if (_TimeSinceBegan > _TimeTillEnd)
                {
                    _Complete = true;
                }
            }
        }

        /// <inheritdoc />
        public override Boolean IsComplete()
        {
#if __ANDROID__
            if (_Complete) ChangeState(new GooglePlayConnectState());
#elif __DESKTOP__
            if (_Complete) ChangeState(new MainMenuState());
#endif
            return _Complete;
        }

        /// <inheritdoc />
        protected override void OnExit()
        {
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _Scene.Dispose();
            _Scene = null;
            base.Dispose();
        }
    }
}
