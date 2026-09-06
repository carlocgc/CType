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
    /// One action on the controls screen, shown as "FIRE        SPACE, Z        A, RT".
    /// Confirming the row waits for the player to press an input and binds the action to it.
    /// </summary>
    public sealed class BindingRow : IFocusable
    {
        /// <summary> Tint applied while the row does not have focus </summary>
        private static readonly Vector4 UnfocusedTint = new Vector4(0.55f, 0.55f, 0.55f, 1);
        /// <summary> Tint applied while the row has focus </summary>
        private static readonly Vector4 FocusedTint = new Vector4(1, 1, 1, 1);
        /// <summary> Tint applied to the message refusing an input that may not be bound </summary>
        private static readonly Vector4 RefusedTint = new Vector4(1, 0.4f, 0.4f, 1);

        /// <summary> Shown in place of the inputs while waiting for the player to press one </summary>
        private const String CapturePrompt = "PRESS INPUT";
        /// <summary> Shown when the input pressed is one the player may not bind </summary>
        private const String RefusedMessage = "RESERVED";

        /// <summary> Where the keyboard column sits relative to the row </summary>
        private static readonly Vector2 KeyColumn = new Vector2(520, 0);
        /// <summary> Where the gamepad column sits relative to the row </summary>
        private static readonly Vector2 PadColumn = new Vector2(1180, 0);

        /// <summary> The action this row binds </summary>
        private readonly ButtonData.Type _Action;
        /// <summary> The action name </summary>
        private readonly TextDisplay _Label;
        /// <summary> The keys bound to the action </summary>
        private readonly TextDisplay _Keys;
        /// <summary> The gamepad buttons bound to the action </summary>
        private readonly TextDisplay _Pad;
        /// <summary> Invoked once a rebind has changed the mapping, so every row can be refreshed </summary>
        private readonly Action _OnChanged;

        /// <summary> Whether the row has focus </summary>
        private Boolean _Focused;
        /// <summary> Whether the row is waiting for the player to press an input </summary>
        private Boolean _Capturing;
        /// <summary> Whether the row is showing that an input may not be bound </summary>
        private Boolean _Refused;
        /// <summary> Whether the row has been torn down </summary>
        private Boolean _Disposed;

        /// <inheritdoc />
        public Boolean CanFocus => true;

        /// <summary>
        /// Creates a row for one action
        /// </summary>
        /// <param name="action"> The action the row binds </param>
        /// <param name="position"> Where to place the left edge of the row </param>
        /// <param name="onChanged">
        /// Invoked after a rebind. A rebind can move an input off another action, so the whole
        /// screen is refreshed rather than this row alone.
        /// </param>
        public BindingRow(ButtonData.Type action, Vector2 position, Action onChanged)
        {
            _Action = action;
            _OnChanged = onChanged;

            _Label = CreateText(InputBindings.DescribeAction(action), position);
            _Keys = CreateText(String.Empty, position + KeyColumn);
            _Pad = CreateText(String.Empty, position + PadColumn);

            Refresh();
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
        /// Re-reads the mapping and shows what the action is currently bound to
        /// </summary>
        public void Refresh()
        {
            if (_Disposed) return;

            _Refused = false;

            if (_Capturing)
            {
                _Keys.Text = CapturePrompt;
                _Pad.Text = String.Empty;
                ApplyTint();
                return;
            }

            InputBindings bindings = InputService.Instance.Bindings;

            _Keys.Text = bindings == null ? InputBindings.Unbound : bindings.DescribeAll(_Action, false);
            _Pad.Text = bindings == null ? InputBindings.Unbound : bindings.DescribeAll(_Action, true);

            ApplyTint();
        }

        /// <summary>
        /// Colours the row for its focus state
        /// </summary>
        private void ApplyTint()
        {
            Vector4 tint = _Focused ? FocusedTint : UnfocusedTint;
            _Label.Colour = tint;
            _Keys.Colour = _Refused ? RefusedTint : tint;
            _Pad.Colour = tint;
        }

        /// <summary>
        /// Says why nothing happened when the player pressed an input that cannot be bound
        /// </summary>
        /// <remarks>
        /// Cleared by moving the cursor or asking to bind again, rather than after a delay. The
        /// controls screen is reachable from the pause menu, where the clock is stopped, so a
        /// message that timed itself out would sit there for the rest of the run.
        /// </remarks>
        private void ShowRefusal()
        {
            _Refused = true;
            _Keys.Text = RefusedMessage;
            _Pad.Text = String.Empty;
            ApplyTint();
        }

        /// <summary>
        /// Applies whatever the player pressed, or leaves the row as it was
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

            if (!ControlSettings.Rebind(_Action, source))
            {
                ShowRefusal();
                return;
            }

            Refresh();
            _OnChanged?.Invoke();
        }

        #region Implementation of IFocusable

        /// <inheritdoc />
        public void SetFocused(Boolean focused)
        {
            _Focused = focused;

            if (_Refused) Refresh();
            else ApplyTint();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Nothing else on the screen reacts while a capture is open: the input provider stops
        /// reporting actions until it closes, so the press that chooses a binding cannot also
        /// move the focus cursor or leave the screen.
        /// </remarks>
        public void Activate()
        {
            if (_Capturing) return;

            _Capturing = true;
            Refresh();
            InputService.Instance.BeginCapture(OnCaptured);
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
            _Keys.Dispose();
            _Pad.Dispose();
        }
    }
}
