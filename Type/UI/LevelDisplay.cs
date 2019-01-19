using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using Type.Base;

namespace Type.UI
{
    /// <summary>
    /// Displays the current level on screen
    /// </summary>
    public class LevelDisplay : GameObject
    {
        /// <summary> Text that displays the current level </summary>
        private readonly TextDisplay _Display;
        /// <summary> Whether the text is being displayed </summary>
        private Boolean _Active;
        /// <summary> How long the text has been displayed for </summary>
        private TimeSpan _DisplayTime;
        /// <summary> How long to display the text for </summary>
        private TimeSpan _TargetDisplayTime;
        /// <summary> Whether the text has finished displaying </summary>
        private Action _OnComplete;

        public LevelDisplay()
        {
            _Display = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(2, 2),
            };
            _Display.Offset = new Vector2(_Display.Size.X * _Display.Scale.X, _Display.Size.Y * _Display.Scale.Y) / 2;
        }

        /// <summary>
        /// Shows the current level as text on the screen, calls the on complete after the given time
        /// </summary>
        /// <param name="level"> Level to show </param>
        /// <param name="duration"> How long to show </param>
        /// <param name="onComplete"> Action to invoke when complete </param>
        public void ShowLevel(Int32 level, TimeSpan duration, Action onComplete)
        {
            _Display.Text = $"LEVEL {level}";
            _Display.Offset = new Vector2(_Display.Size.X * _Display.Scale.X, _Display.Size.Y * _Display.Scale.Y) / 2;
            _Display.Visible = true;
            _TargetDisplayTime = duration;
            _OnComplete = onComplete;
            _Active = true;
        }

        /// <summary>
        /// Hides the display and calls the complete action
        /// </summary>
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
