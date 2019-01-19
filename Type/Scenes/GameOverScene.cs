using System;
using AmosShared.Audio;
using AmosShared.Base;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using OpenTK;

namespace Type.Scenes
{
    /// <summary>
    /// The Game over scene
    /// </summary>
    public class GameOverScene : Scene
    {
        private TextDisplay _GameOverText;

        /// <summary> Text that prompts user to restart </summary>
        private TextDisplay _ScoreText;

        private Button _ConfirmButton;

        public Boolean IsComplete { get; set; }

        public GameOverScene()
        {
            _GameOverText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "GAME OVER",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1700, -Renderer.Instance.TargetDimensions.Y / 2 + 500),
                Visible = true,
                Scale = new Vector2(11, 11),
                Colour = new Vector4(1, 0, 0, 1)
            };
            AddDrawable(_GameOverText);
            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(-730, -200),
                Visible = true,
                Scale = new Vector2(5, 5),
                TextAlignment = TextDisplay.Alignment.CENTER,
                Colour = new Vector4(1, 1, 1, 1)
            };
            AddDrawable(_ScoreText);

            Sprite confirmSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/GameOverBG.png"))
            {
                Position = new Vector2(-960, -540),
                Colour = new Vector4(0.5f, 0.5f, 0.5f, 1)
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmSprite);
            _ConfirmButton.OnButtonPress += OnButtonPress;
        }

        private void OnButtonPress(Button button)
        {
            _ConfirmButton.TouchEnabled = false;
            _ConfirmButton.Visible = false;
            IsComplete = true;
        }

        public void Show(Int32 score)
        {
            _ScoreText.Text = $"SCORE {score}";
            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;
            new AudioPlayer("Content/Audio/gameOver.wav", false, AudioManager.Category.EFFECT, 1);
        }

        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _ConfirmButton.Dispose();
        }
    }
}
