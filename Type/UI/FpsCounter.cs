using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;

namespace Type.UI
{
    public class FpsCounter : GameObject
    {
        private Int32 _Counter;

        private DateTime _TimeSinceLastUpdate;

        private TextDisplay _Display;

        private TimeSpan _ToggleTimer;

        private Boolean _ToggleOnCooldown;

        public FpsCounter()
        {

            _Display = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 100,
                    -Renderer.Instance.TargetDimensions.Y / 2 + 60),
                Visible = true
            };
            _Display.Visible = Constants.Global.SHOW_FPS;
        }

        public void ToggleVisibility()
        {
            if (_ToggleOnCooldown) return;

            _Display.Visible = !_Display.Visible;

            _ToggleOnCooldown = true;
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            _Counter++;
            if ((DateTime.Now - _TimeSinceLastUpdate).TotalSeconds >= 1)
            {
                _Display.Text = $"{_Counter}";
                _TimeSinceLastUpdate = DateTime.Now;
                _Counter = 0;
            }

            if (_ToggleOnCooldown)
            {
                _ToggleTimer += timeTilUpdate;
                if (_ToggleTimer >= TimeSpan.FromSeconds(1))
                {
                    _ToggleOnCooldown = false;
                    _ToggleTimer = TimeSpan.Zero;
                }
            }
        }
    }
}
