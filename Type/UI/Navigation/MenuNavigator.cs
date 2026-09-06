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
        /// <summary> Whether the held direction came from a horizontal input </summary>
        private Boolean _HeldHorizontal;
        /// <summary> How long the current direction has been held </summary>
        private TimeSpan _HeldFor;
        /// <summary> How long since the last repeat was emitted </summary>
        private TimeSpan _SinceRepeat;
        /// <summary> Whether the held direction has begun repeating </summary>
        private Boolean _Repeating;
        /// <summary> Whether the stick was pushed past the threshold last update </summary>
        private Boolean _StickPushed;
        /// <summary> The column focus was last on, carried between grid items as focus moves </summary>
        private Int32 _Column;

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
        /// <param name="horizontal"> Whether the input was left or right rather than up or down </param>
        private void SetHeldDirection(Int32 direction, Boolean horizontal = false)
        {
            if (_HeldDirection == direction && _HeldHorizontal == horizontal) return;

            _HeldDirection = direction;
            _HeldHorizontal = horizontal;
            _HeldFor = TimeSpan.Zero;
            _SinceRepeat = TimeSpan.Zero;
            _Repeating = false;

            if (direction != 0) Apply();
        }

        /// <summary>
        /// Starts or stops holding a direction in response to one directional input
        /// </summary>
        /// <param name="state"> The state the input was reported in </param>
        /// <param name="direction"> -1 for previous or decrease, 1 for next or increase </param>
        /// <param name="horizontal"> Whether the input was left or right </param>
        private void HandleDirection(ButtonData.State state, Int32 direction, Boolean horizontal)
        {
            if (state == ButtonData.State.PRESSED)
            {
                SetHeldDirection(direction, horizontal);
                return;
            }

            // Only the input that started the hold may end it. Up and left share a direction, so
            // releasing one must not cancel a hold begun by the other.
            if (state != ButtonData.State.RELEASED) return;
            if (_HeldDirection != direction || _HeldHorizontal != horizontal) return;

            SetHeldDirection(0);
        }

        /// <summary>
        /// Applies the held direction: left and right adjust the focused item when it holds a
        /// value, and otherwise move focus along with up and down.
        /// </summary>
        /// <remarks>
        /// Moving between two grid items keeps the column, so a list of them reads as a table:
        /// stepping down from the third cell of one row lands on the third cell of the next
        /// rather than back at its start. The column is remembered rather than read from the
        /// item arrived at, so passing over an entry that has no columns — the reset entry at
        /// the foot of the controls screen — and coming back returns to the column left behind.
        /// </remarks>
        private void Apply()
        {
            if (_HeldHorizontal && _Ring.Focused is IAdjustable adjustable)
            {
                adjustable.Adjust(_HeldDirection);

                // Read back rather than predicted, so a step refused at either end of a row does
                // not leave a column remembered that the row never actually reached.
                if (adjustable is IGridFocusable moved) _Column = moved.Column;
                return;
            }

            _Ring.Move(_HeldDirection);

            if (_Ring.Focused is IGridFocusable grid) grid.Column = _Column;
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
                Apply();
                return;
            }

            _SinceRepeat += timeTilUpdate;
            if (_SinceRepeat < RepeatInterval) return;
            _SinceRepeat = TimeSpan.Zero;
            Apply();
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
                case ButtonData.Type.MENU_UP:
                    {
                        HandleDirection(data.State, -1, false);
                        break;
                    }
                case ButtonData.Type.MENU_DOWN:
                    {
                        HandleDirection(data.State, 1, false);
                        break;
                    }
                case ButtonData.Type.MENU_LEFT:
                    {
                        HandleDirection(data.State, -1, true);
                        break;
                    }
                case ButtonData.Type.MENU_RIGHT:
                    {
                        HandleDirection(data.State, 1, true);
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
            if (pushed && !_StickPushed) SetHeldDirection(horizontal > 0 ? 1 : -1, true);
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
