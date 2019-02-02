using System;
using AmosShared.Base;
using AmosShared.Interfaces;
using Android.Hardware;

namespace Type.Base
{
    /// <summary>
    /// Invokes an action after a given duration
    /// </summary>
    public class TimedCallback : IUpdatable
    {
        /// <summary> How long to wait before calling the callback </summary>
        private readonly TimeSpan _Duration;
        /// <summary> How long has past since the timer started </summary>
        private TimeSpan _Elapsed;
        /// <summary> The callback to invoke </summary>
        private Action _Callback;
        /// <summary> Whether the call back has been invoked </summary>
        private Boolean _Complete;

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        public TimedCallback(TimeSpan duration, Action callback)
        {
            _Duration = duration;
            _Callback = callback;
            _Elapsed = TimeSpan.Zero;
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Calls the callback action and cancels the timer
        /// </summary>
        public void CancelAndComplete()
        {
            _Callback?.Invoke();
            Dispose();
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            _Elapsed += timeTilUpdate;

            if (_Elapsed < _Duration && !_Complete) return;
            _Callback.Invoke();
            _Complete = true;
            Dispose();
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return true;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            UpdateManager.Instance.RemoveUpdatable(this);
            _Callback = null;
            IsDisposed = true;
        }
    }
}
