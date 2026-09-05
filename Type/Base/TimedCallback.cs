using AmosShared.Base;
using AmosShared.Interfaces;
using System;

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
            if (IsDisposed) return;

            _Callback?.Invoke();
            Dispose();
        }

        /// <summary> Called to update the object </summary>
        /// <param name="timeTilUpdate"></param>
        /// <remarks>
        /// Refuses to run once disposed. <see cref="Dispose"/> drops the callback, so an update
        /// arriving after it would otherwise throw on the invoke. Nothing delivers one today —
        /// `UpdateManager` skips anything already queued for removal — but that is the engine's
        /// guarantee rather than this class's, and a timer that fires an action it was told to
        /// forget is the wrong shape to leave lying around.
        /// </remarks>
        public void Update(TimeSpan timeTilUpdate)
        {
            if (IsDisposed) return;

            _Elapsed += timeTilUpdate;

            if (_Elapsed < _Duration && !_Complete) return;
            _Callback?.Invoke();
            _Complete = true;
            Dispose();
        }

        /// <summary> Whether or not the object can be updated </summary>
        /// <returns></returns>
        public Boolean CanUpdate()
        {
            return !IsDisposed;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            UpdateManager.Instance.RemoveUpdatable(this);
            _Callback = null;
            IsDisposed = true;
        }
    }
}
