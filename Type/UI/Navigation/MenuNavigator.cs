using AmosShared.Base;
using AmosShared.Interfaces;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.Interfaces.Control;
using Type.Services;

namespace Type.UI.Navigation
{
    /// <summary>
    /// Moves focus between a list of <see cref="IFocusable"/> items and activates the focused
    /// one, so that a menu can be driven entirely from a gamepad or the keyboard.
    /// </summary>
    /// <remarks>
    /// Touch and mouse input are untouched. The engine's buttons keep handling those
    /// themselves, so a menu remains usable by pointer as well.
    /// </remarks>
    public sealed class MenuNavigator : IInputListener, IUpdatable
    {
        /// <summary> How long a direction must be held before it starts repeating </summary>
        private static readonly TimeSpan RepeatDelay = TimeSpan.FromMilliseconds(400);
        /// <summary> How long between repeats once repeating has started </summary>
        private static readonly TimeSpan RepeatInterval = TimeSpan.FromMilliseconds(140);
        /// <summary> Stick deflection past which a direction counts as pushed </summary>
        private const Single StickThreshold = 0.6f;

        /// <summary> Focus order and which item currently holds focus </summary>
        private readonly FocusRing _Ring = new FocusRing();

        /// <summary> Direction currently being held, zero when none </summary>
        private Int32 _HeldDirection;
        /// <summary> How long the current direction has been held </summary>
        private TimeSpan _HeldFor;
        /// <summary> How long since the last repeat was emitted </summary>
        private TimeSpan _SinceRepeat;
        /// <summary> Whether the held direction has begun repeating </summary>
        private Boolean _Repeating;
        /// <summary> Whether the stick was pushed past the threshold last update </summary>
        private Boolean _StickPushed;

        /// <summary> Invoked when the player backs out of the menu </summary>
        public Action OnCancel { get; set; }

        /// <summary> The focused item, or null when nothing is focused </summary>
        public IFocusable Focused => _Ring.Focused;

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        public MenuNavigator()
        {
            InputService.Instance.RegisterListener(this);
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Adds an item to the end of the navigation order
        /// </summary>
        /// <param name="item"> The item focus can move to </param>
        public void Add(IFocusable item)
        {
            _Ring.Add(item);
        }

        /// <summary>
        /// Moves focus to the first item that can take it. Called once the menu is on screen.
        /// </summary>
        public void FocusFirst()
        {
            _Ring.FocusFirst();
        }


        /// <summary>
        /// Begins or ends holding a direction. Emits one move immediately, then repeats while
        /// the direction stays held.
        /// </summary>
        /// <param name="direction"> -1 for previous, 1 for next, 0 to stop </param>
        private void SetHeldDirection(Int32 direction)
        {
            if (_HeldDirection == direction) return;

            _HeldDirection = direction;
            _HeldFor = TimeSpan.Zero;
            _SinceRepeat = TimeSpan.Zero;
            _Repeating = false;

            if (direction != 0) _Ring.Move(direction);
        }

        #region Implementation of IUpdatable

        /// <inheritdoc />
        public void Update(TimeSpan timeTilUpdate)
        {
            if (_HeldDirection == 0) return;

            if (!_Repeating)
            {
                _HeldFor += timeTilUpdate;
                if (_HeldFor < RepeatDelay) return;
                _Repeating = true;
                _SinceRepeat = TimeSpan.Zero;
                _Ring.Move(_HeldDirection);
                return;
            }

            _SinceRepeat += timeTilUpdate;
            if (_SinceRepeat < RepeatInterval) return;
            _SinceRepeat = TimeSpan.Zero;
            _Ring.Move(_HeldDirection);
        }

        /// <inheritdoc />
        public Boolean CanUpdate()
        {
            return true;
        }

        #endregion

        #region Implementation of IInputListener

        /// <inheritdoc />
        public void UpdateInputData(ButtonEventData data)
        {
            switch (data.ID)
            {
                case ButtonData.Type.MENU_LEFT:
                case ButtonData.Type.MENU_UP:
                    {
                        if (data.State == ButtonData.State.PRESSED) SetHeldDirection(-1);
                        else if (data.State == ButtonData.State.RELEASED && _HeldDirection == -1) SetHeldDirection(0);
                        break;
                    }
                case ButtonData.Type.MENU_RIGHT:
                case ButtonData.Type.MENU_DOWN:
                    {
                        if (data.State == ButtonData.State.PRESSED) SetHeldDirection(1);
                        else if (data.State == ButtonData.State.RELEASED && _HeldDirection == 1) SetHeldDirection(0);
                        break;
                    }
                case ButtonData.Type.CONFIRM:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        Focused?.Activate();
                        break;
                    }
                case ButtonData.Type.CANCEL:
                    {
                        if (data.State != ButtonData.State.PRESSED) return;
                        OnCancel?.Invoke();
                        break;
                    }
            }
        }

        /// <summary>
        /// Allows the analog stick to move focus as well as the D-pad and the keyboard
        /// </summary>
        /// <param name="direction"> The direction the stick is pushed </param>
        /// <param name="strength"> The distance the stick is pushed </param>
        public void UpdateDirectionData(Vector2 direction, Single strength)
        {
            Single horizontal = direction.X * strength;
            Boolean pushed = Math.Abs(horizontal) >= StickThreshold;

            // Only react to the stick crossing the threshold, so that a stick left resting off
            // centre does not fight the D-pad for the held direction.
            if (pushed && !_StickPushed) SetHeldDirection(horizontal > 0 ? 1 : -1);
            else if (!pushed && _StickPushed && _HeldDirection != 0) SetHeldDirection(0);

            _StickPushed = pushed;
        }

        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            InputService.Instance.DeregisterListener(this);
            UpdateManager.Instance.RemoveUpdatable(this);
            _Ring.Clear();
            OnCancel = null;
        }
    }
}
