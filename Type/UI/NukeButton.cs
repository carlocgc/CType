using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;
using Type.Interfaces;
using Type.Interfaces.Control;

namespace Type.UI
{
    /// <summary>
    /// Button that can detonate a nuke, displays the current nuke count
    /// </summary>
    public class NukeButton : GameObject, INotifier<INukeButtonListener>
    {
        /// <summary> All the objects listening to this objects </summary>
        private readonly List<INukeButtonListener> _Listeners = new List<INukeButtonListener>();
        /// <summary> Text showing how many nukes the player has </summary>
        private readonly TextDisplay _NukeCountDisplay;
        /// <summary> Button that will inform the listeners </summary>
        private readonly Button _Button;
        /// <summary> The number of nukes to display on the button </summary>
        private Int32 _NukeCount;

        /// <summary> The number of nukes to display on the button </summary>
        public Int32 NukeCount
        {
            get => _NukeCount;
            set
            {
                _NukeCount = value;
                if (_NukeCount > 0)
                {
                    _Button.Sprite.Colour = new Vector4(1, 1, 1, 0.4f);
                    _Button.TouchEnabled = true;
                    _NukeCountDisplay.Visible = true;
                }
                else
                {
                    _Button.Sprite.Colour = new Vector4(0.4f, 0.4f, 0.4f, 0.4f);
                    _Button.TouchEnabled = false;
                    _NukeCountDisplay.Visible = false;
                }
                _NukeCountDisplay.Text = $"{_NukeCount}";
            }
        }

        public NukeButton()
        {
            Sprite buttonSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/Buttons/nuke_button.png"))
            {
                Position = new Vector2(425, -450),
                Visible = true,
            };
            _Button = new Button(Int32.MaxValue, buttonSprite) { OnButtonPress = OnPressed };

            _NukeCountDisplay = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI_OVERLAY, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"{NukeCount}",
                Position = _Button.Position + _Button.Sprite.Size / 2,
                Scale = new Vector2(2, 2),
            };
            _NukeCountDisplay.Offset = new Vector2(_NukeCountDisplay.Size.X * _NukeCountDisplay.Scale.X, _NukeCountDisplay.Size.Y * _NukeCountDisplay.Scale.Y) / 2;
            NukeCount = 0;
        }

        /// <summary>
        /// Invoked when the nuke button is pressed, informs the listeners a press has occurred
        /// </summary>
        /// <param name="obj"></param>
        private void OnPressed(Button obj)
        {
            foreach (INukeButtonListener listener in _Listeners)
            {
                listener.OnNukeButtonPressed();
            }
        }

        #region Implementation of INotifier<in INukeButtonListener>

        /// <summary>
        /// Add a listener
        /// </summary>
        public void RegisterListener(INukeButtonListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        /// <summary>
        /// Remove a listener
        /// </summary>
        public void DeregisterListener(INukeButtonListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

        #endregion

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _Button.Dispose();
            _NukeCountDisplay.Dispose();
            _Listeners.Clear();
        }

        #endregion
    }
}
