using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using AmosShared.Touch;
using System;
using AmosShared.Audio;
using AmosShared.Base;
using OpenTK;

namespace Type.Scenes
{
    public class GameCompleteScene : Scene
    {
        /// <summary> Button used to return to main menu </summary>
        private readonly Button _ConfirmButton;
        /// <summary> Text displaying the word congratulations </summary>
        private readonly TextDisplay _CongratsText;
        /// <summary> Text that prompts user to restart </summary>
        private readonly TextDisplay _ScoreText;

        /// <summary> Whether the complete can end </summary>
        public Boolean IsComplete { get; set; }

        public GameCompleteScene()
        {
            _CongratsText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = "CONGRATULATIONS",
                Position = new Vector2(0, 0),
                Visible = true,
                Scale = new Vector2(6, 6),
                Colour = new Vector4(0, 0, 1, 1)
            };
            _CongratsText.Offset = new Vector2(_CongratsText.Size.X * _CongratsText.Scale.X, _CongratsText.Size.Y * _CongratsText.Scale.Y) / 2;
            AddDrawable(_CongratsText);
            _ScoreText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Position =  new Vector2(0, -200),
                Visible = true,
                Scale = new Vector2(5, 5),
                Colour = new Vector4(1, 1, 1, 1)
            };
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;
            AddDrawable(_ScoreText);

            Sprite confirmSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/GameOverBG.png"))
            {
                Position = new Vector2(-960, -540),
                Colour = new Vector4(0.5f, 0.5f, 0.5f, 1)
            };
            _ConfirmButton = new Button(Constants.ZOrders.UI, confirmSprite);
            _ConfirmButton.OnButtonPress += OnButtonPress;

            new AudioPlayer("Content/Audio/gameCompleteBgm.wav", true, AudioManager.Category.MUSIC, 1);
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

        public void Start(Int32 score)
        {
            _ScoreText.Text = $"SCORE {score}";
            _ScoreText.Offset = new Vector2(_ScoreText.Size.X * _ScoreText.Scale.X, _ScoreText.Size.Y * _ScoreText.Scale.Y) / 2;

            _ConfirmButton.TouchEnabled = true;
            _ConfirmButton.Visible = true;
        }

        /// <summary> Disposes of the scene </summary>
        public override void Dispose()
        {
            base.Dispose();
            _ConfirmButton.Dispose();
            AudioManager.Instance.Dispose();
        }
    }
}
