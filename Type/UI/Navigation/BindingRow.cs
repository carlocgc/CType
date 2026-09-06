using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Data;
using Type.Input;
using Type.Interfaces.Control;
using Type.Services;

namespace Type.UI.Navigation
{
    /// <summary>
    /// One action on the controls screen, shown as its name followed by a cell per binding:
    /// two for the keyboard and two for the gamepad. Left and right move between the cells,
    /// and confirming one waits for the player to press an input to put in it.
    /// </summary>
    /// <remarks>
    /// A cell per slot rather than one per device, because the defaults bind two of each — Space
    /// and Z both fire — and a screen that could only set one of them collapsed that the moment
    /// it was touched, with no way back short of resetting everything.
    /// </remarks>
    public sealed class BindingRow : IAdjustable
    {
        /// <summary> Tint applied while the row does not have focus </summary>
        private static readonly Vector4 UnfocusedTint = new Vector4(0.55f, 0.55f, 0.55f, 1);
        /// <summary> Tint applied to the focused row, other than the cell the cursor is on </summary>
        private static readonly Vector4 FocusedTint = new Vector4(0.8f, 0.8f, 0.8f, 1);
        /// <summary> Tint applied to the cell the cursor is on </summary>
        private static readonly Vector4 SelectedTint = new Vector4(1, 1, 1, 1);
        /// <summary> Tint applied to the message refusing an input that may not be bound </summary>
        private static readonly Vector4 RefusedTint = new Vector4(1, 0.4f, 0.4f, 1);

        /// <summary> Shown in a cell while waiting for the player to press an input for it </summary>
        private const String CapturePrompt = "PRESS";
        /// <summary> Shown when the input pressed is one this action may not take </summary>
        private const String RefusedMessage = "TAKEN";

        /// <summary> Number of cells in a row, one per slot per device </summary>
        private const Int32 CellCount = InputBindings.Slots * 2;

        /// <summary> The action this row binds </summary>
        private readonly ButtonData.Type _Action;
        /// <summary> The action name </summary>
        private readonly TextDisplay _Label;
        /// <summary> One text per binding cell, keyboard slots first then gamepad slots </summary>
        private readonly TextDisplay[] _Cells = new TextDisplay[CellCount];
        /// <summary> Invoked once a rebind has changed the mapping, so every row can be refreshed </summary>
        private readonly Action _OnChanged;

        /// <summary> Which cell the cursor is on </summary>
        private Int32 _Selected;
        /// <summary> Whether the row has focus </summary>
        private Boolean _Focused;
        /// <summary> Whether the row is waiting for the player to press an input </summary>
        private Boolean _Capturing;
        /// <summary> Whether the row is showing that an input may not be taken </summary>
        private Boolean _Refused;
        /// <summary> Whether the row has been torn down </summary>
        private Boolean _Disposed;

        /// <inheritdoc />
        public Boolean CanFocus => true;

        /// <summary>
        /// Creates a row for one action
        /// </summary>
        /// <param name="action"> The action the row binds </param>
        /// <param name="position"> Where to place the left edge of the label </param>
        /// <param name="columns"> Distance from the label to each cell, one per cell </param>
        /// <param name="onChanged">
        /// Invoked after a rebind. A rebind can move an input off another action, so the whole
        /// screen is refreshed rather than this row alone.
        /// </param>
        public BindingRow(ButtonData.Type action, Vector2 position, Single[] columns, Action onChanged)
        {
            _Action = action;
            _OnChanged = onChanged;

            _Label = CreateText(InputBindings.DescribeAction(action), position);

            for (Int32 cell = 0; cell < CellCount; cell++)
            {
                _Cells[cell] = CreateText(String.Empty, position + new Vector2(columns[cell], 0));
            }

            Refresh();
        }

        /// <summary>
        /// Whether a cell holds a gamepad button rather than a key. The keyboard slots come
        /// first, so the row reads left to right in the order the columns are headed.
        /// </summary>
        private static Boolean IsGamepadCell(Int32 cell)
        {
            return cell >= InputBindings.Slots;
        }

        /// <summary>
        /// Which of the action's inputs for that cell's device the cell holds
        /// </summary>
        private static Int32 SlotOf(Int32 cell)
        {
            return cell % InputBindings.Slots;
        }

        /// <summary>
        /// Creates one piece of row text
        /// </summary>
        private static TextDisplay CreateText(String text, Vector2 position)
        {
            return new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY,
                Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = text,
                Position = position,
                Visible = true,
                Scale = new Vector2(2, 2),
                Colour = UnfocusedTint,
            };
        }

        /// <summary>
        /// Re-reads the mapping and shows what each cell holds
        /// </summary>
        public void Refresh()
        {
            if (_Disposed) return;

            _Refused = false;

            InputBindings bindings = InputService.Instance.Bindings;

            for (Int32 cell = 0; cell < CellCount; cell++)
            {
                if (_Capturing && cell == _Selected)
                {
                    _Cells[cell].Text = CapturePrompt;
                    continue;
                }

                _Cells[cell].Text = bindings == null
                    ? InputBindings.Unbound
                    : bindings.DescribeSlot(_Action, IsGamepadCell(cell), SlotOf(cell));
            }

            ApplyTint();
        }

        /// <summary>
        /// Colours the row for its focus state, picking out the cell the cursor is on
        /// </summary>
        private void ApplyTint()
        {
            _Label.Colour = _Focused ? FocusedTint : UnfocusedTint;

            for (Int32 cell = 0; cell < CellCount; cell++)
            {
                if (!_Focused)
                {
                    _Cells[cell].Colour = UnfocusedTint;
                    continue;
                }

                if (cell != _Selected) _Cells[cell].Colour = FocusedTint;
                else _Cells[cell].Colour = _Refused ? RefusedTint : SelectedTint;
            }
        }

        /// <summary>
        /// Says why nothing happened when the player pressed an input this action cannot take
        /// </summary>
        /// <remarks>
        /// Cleared by moving the cursor or asking to bind again, rather than after a delay. The
        /// controls screen is reachable from the pause menu, where the clock is stopped, so a
        /// message that timed itself out would sit there for the rest of the run.
        /// </remarks>
        private void ShowRefusal()
        {
            _Refused = true;
            _Cells[_Selected].Text = RefusedMessage;
            ApplyTint();
        }

        /// <summary>
        /// Applies whatever the player pressed to the selected cell, or leaves the row as it was
        /// </summary>
        /// <param name="source"> The input pressed, or null if the player backed out </param>
        private void OnCaptured(InputSource source)
        {
            if (_Disposed) return;

            _Capturing = false;

            if (source == null)
            {
                Refresh();
                return;
            }

            if (!ControlSettings.Rebind(_Action, IsGamepadCell(_Selected), SlotOf(_Selected), source))
            {
                ShowRefusal();
                return;
            }

            Refresh();
            _OnChanged?.Invoke();
        }

        #region Implementation of IAdjustable

        /// <inheritdoc />
        public void SetFocused(Boolean focused)
        {
            _Focused = focused;

            if (_Refused) Refresh();
            else ApplyTint();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Moves the cursor between the row's cells rather than changing a value, and stops at
        /// each end rather than wrapping, so left and right stay inside the row and up and down
        /// remain the only way off it.
        /// </remarks>
        public void Adjust(Int32 direction)
        {
            if (_Capturing) return;

            Int32 next = _Selected + direction;
            if (next < 0 || next >= CellCount) return;

            _Selected = next;
            Refresh();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Nothing else on the screen reacts while a capture is open: the input provider stops
        /// reporting actions until it closes, so the press that chooses a binding cannot also
        /// move the focus cursor or leave the screen. Only the cell's own device is listened
        /// for, so pressing a key at a gamepad cell leaves the prompt up rather than binding
        /// something the cell cannot hold.
        /// </remarks>
        public void Activate()
        {
            if (_Capturing) return;

            _Capturing = true;
            Refresh();
            InputService.Instance.BeginCapture(IsGamepadCell(_Selected), OnCaptured);
        }

        #endregion

        /// <summary>
        /// Disposes the row's text, abandoning a capture it left open
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            // The provider reports an abandoned capture synchronously, and this row is going
            // away, so the disposed flag is set first and the callback returns without touching
            // text that is about to be destroyed.
            if (_Capturing) InputService.Instance.CancelCapture();
            _Capturing = false;

            _Label.Dispose();
            foreach (TextDisplay cell in _Cells) cell.Dispose();
        }
    }
}
