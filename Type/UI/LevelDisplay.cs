using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;

namespace Type.UI
{
    public class LevelDisplay : GameObject
    {
        private readonly TextDisplay _Display;

        private Boolean _Active;

        private TimeSpan _DisplayTime;

        private TimeSpan _TargetDisplayTime;

        private Action _OnComplete;

        public LevelDisplay()
        {
            _Display = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Scale = new Vector2(3, 3),
            };
            _Display.Position = new Vector2(-_Display.Width/ 2, 0);
        }

        public void ShowLevel(Int32 level, TimeSpan duration, Action onComplete)
        {
            _Display.Text = $"LEVEL {level} START";
            _Display.Visible = true;
            _TargetDisplayTime = duration;
            _OnComplete = onComplete;
            _Active = true;
        }

        private void Complete()
        {
            _Display.Visible = false;
            _DisplayTime = TimeSpan.Zero;
            _TargetDisplayTime = TimeSpan.Zero;
            _OnComplete.Invoke();
            _Active = false;
            //new AudioPlayer("Content/Audio/begin.wav", false, AudioManager.Category.EFFECT, 1);
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
            if (!_Active) return;

            _DisplayTime += timeTilUpdate;

            if (_DisplayTime >= _TargetDisplayTime)
            {
                Complete();
            }
        }
    }
}
