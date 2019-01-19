using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using System;
using AmosShared.Base;
using OpenTK;

namespace Type.Scenes
{
    public class GameCompleteScene : Scene
    {
        /// <summary> The bacground sprite </summary>
        private Sprite _Background;
        /// <summary> Text displaying the word congratulations </summary>
        private TextDisplay _CongratsText;
        /// <summary> Text that prompts user to restart </summary>
        private TextDisplay _ScoreText;
        /// <summary> Button used to return to main menu </summary>
        private Button _ConfirmButton;

        /// <summary> Whether the complete can end </summary>
        public Boolean IsComplete { get; set; }

        public GameCompleteScene()
        {
            _CongratsText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "CONGRATULATIONS",
                Position = new Vector2(Renderer.Instance.TargetDimensions.X / 2 - 1700, -Renderer.Instance.TargetDimensions.Y / 2 + 500),
                Visible = true,
                Scale = new Vector2(11, 11),
                Colour = new Vector4(1, 0, 1, 1)
            };
            AddDrawable(_CongratsText);
            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position = new Vector2(-730, -200),
                Visible = true,
                Scale = new Vector2(5, 5),
                TextAlignment = TextDisplay.Alignment.CENTER,
                Colour = new Vector4(1, 1, 1, 1)
            };
            AddDrawable(_ScoreText);

            Sprite confirmSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/GameCompletBG.png"))
            {
                Position = new Vector2(-960, -540),
                Colour = new Vector4(0.5f, 0.5f, 0.5f, 1)
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmSprite);
            _ConfirmButton.OnButtonPress += OnButtonPress;
        }

        private void OnButtonPress(Button obj)
        {
            IsComplete = true;
        }

        /// <summary> Updates the scene </summary>
        /// <param name="timeSinceUpdate"></param>
        public override void Update(TimeSpan timeSinceUpdate)
        {

        }

        public void Show(Int32 score)
        {
            _ScoreText.Text = $"SCORE {score}";
            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;
        }

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _ConfirmButton.Dispose();
        }
    }
}
