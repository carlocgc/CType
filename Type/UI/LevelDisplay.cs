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
        /// <summary> Whether the text has finished displaying </summary>
        private Action _OnComplete;
        /// <summary> Callback from when the text is hidden </summary>
        private TimedCallback _ShownCallback;
        /// <summary> Callback to invoke the complete action </summary>
        private TimedCallback _CompleteCallback;

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
            _OnComplete = onComplete;
            _ShownCallback = new TimedCallback(TimeSpan.FromSeconds(2), Complete);
        }

        /// <summary>
        /// Hides the display and calls the complete action
        /// </summary>
        private void Complete()
        {
            _Display.Visible = false;
            _CompleteCallback = new TimedCallback(TimeSpan.FromSeconds(1), _OnComplete.Invoke);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            _ShownCallback?.Dispose();
            _CompleteCallback?.Dispose();
            _Display.Dispose();
            _OnComplete = null;
            _ShownCallback = null;
            _CompleteCallback = null;
        }
    }
}
