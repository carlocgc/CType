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
    public class ResumeButton : GameObject, IVirtualButton
    {
        private readonly Button _ResumeButton;

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
                _ResumeButton.Visible = value;
            }
        }

        public Boolean Active
        {
            get => _Active;
            set
            {
                _Active = value;
                _ResumeButton.TouchEnabled = value;
            }
        }

        public ResumeButton()
        {
            Sprite resumeButton = new Sprite(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/playbutton.png"))
            {
                Position = new Vector2(770, 350),
                Visible = false,
                Colour = new Vector4(1, 1, 1, 0.4f)
            };
            _ResumeButton = new Button(Int32.MaxValue, resumeButton) { OnButtonPress = ResumeButtonOnPress, OnButtonRelease = ResumeButtonOnRelease };
        }

        private void ResumeButtonOnPress(Button obj)
        {
            State = VirtualButtonData.State.PRESSED;
        }

        private void ResumeButtonOnRelease(Button obj)
        {
            State = VirtualButtonData.State.RELEASED;
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _ResumeButton.Dispose();
        }

        #endregion
    }
}
