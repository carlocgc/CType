using System;
using AmosShared.Interfaces;

namespace Type.Utility
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
        /// <summary> Whether the timer is counting </summary>
        private Boolean _Updating;

        /// <summary> Whether or not the updatable is disposed </summary>
        public Boolean IsDisposed { get; set; }

        public TimedCallback(TimeSpan duration, Action callback)
        {
            _Duration = duration;
            _Callback = callback;
            _Elapsed = TimeSpan.Zero;
            _Updating = true;
        }

        /// <summary>
        /// Calls the callback action and cancels the timer
        /// </summary>
        public void CancelAndComplete()
        {
            _Callback.Invoke();
            Dispose();
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        public void Update(TimeSpan timeTilUpdate)
        {
            if (!_Updating) return;
            _Elapsed += timeTilUpdate;

            if (_Elapsed < _Duration) return;
            _Callback.Invoke();
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
            _Updating = false;
            _Callback = null;
        }
    }
}
