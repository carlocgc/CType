using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using Type.Base;
using Type.Data;
using Type.Interfaces.Control;

namespace Type.Buttons
{
    public class PauseButton : GameObject, IVirtualButton
    {
        private readonly Button _PauseButton;

        private Boolean _Visible;

        private Boolean _Active;

        #region Implementation of IVirtualButton

        /// <summary> The current press state of the button </summary>
        public VirtualButtonData.State State { get; set; }

        #endregion

        public Boolean Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                _PauseButton.Visible = value;
            }
        }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                _PauseButton.TouchEnabled = value;
            }
        }

        public PauseButton()
        {
            Sprite pauseButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/pausebutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _PauseButton = new Button(Int32.MaxValue, pauseButton) { OnButtonPress = PauseButtonOnPress, OnButtonRelease = PauseButtonOnRelease };
        }

        private void PauseButtonOnPress(Button obj)
        {
            State = VirtualButtonData.State.PRESSED;
        }

        private void PauseButtonOnRelease(Button obj)
        {
            State = VirtualButtonData.State.RELEASED;
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _PauseButton.Dispose();
        }

        #endregion
    }
}
