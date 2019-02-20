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
    public class FireButton : GameObject, IVirtualButton
    {
        private readonly Button _FireButton;

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
                _FireButton.Visible = value;
            }
        }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                _FireButton.TouchEnabled = value;
            }
        }

        public FireButton()
        {
            Sprite fireButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/fire.png"))
            {
                Position = new Vector2(625, -450),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _FireButton = new Button(Int32.MaxValue, fireButton) { OnButtonPress = FireButtonPress, OnButtonRelease = FireButtonRelease };
            _FireButton.TouchOrder = fireButton.ZOrder;
        }

        private void FireButtonPress(Button obj)
        {
            State = VirtualButtonData.State.PRESSED;
        }

        private void FireButtonRelease(Button obj)
        {
            State = VirtualButtonData.State.RELEASED;
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _FireButton.Dispose();
        }

        #endregion
    }
}
