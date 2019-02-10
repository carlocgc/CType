using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System.Collections.Generic;
using Type.Base;

namespace Type.UI
{
    /// <summary>
    /// Shows the effects of each pickup item
    /// </summary>
    public class PowerUpHelp : GameObject
    {
        private readonly List<Sprite> _Sprites = new List<Sprite>();

        private readonly List<TextDisplay> _TextDisplays = new List<TextDisplay>();

        private Sprite _PointsSprite;

        private Sprite _AmmoSprite;

        private Sprite _NukeSprite;

        private Sprite _HeartSprite;

        private Sprite _ShieldSprite;

        private TextDisplay _PointText;

        private TextDisplay _AmmoText;

        private TextDisplay _NukeText;

        private TextDisplay _HeartText;

        private TextDisplay _ShieldText;

        public PowerUpHelp()
        {
            _Sprites.Add(_PointsSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Powerups/points_powerup.png"))
            {
                Position = new Vector2(-900, -50)
            });
            _Sprites.Add(_ShieldSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Powerups/shield_powerup.png"))
            {
                Position = new Vector2(-900, -150)
            });
            _Sprites.Add(_AmmoSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Powerups/weapon_powerup.png"))
            {
                Position = new Vector2(-900, -250)
            });
            _Sprites.Add(_NukeSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Powerups/nuke_powerup.png"))
            {
                Position = new Vector2(-900, -350)
            });
            _Sprites.Add(_HeartSprite = new Sprite(Game.MainCanvas, Constants.ZOrders.ABOVE_GAME, Texture.GetTexture("Content/Graphics/Powerups/extralife_powerup.png"))
            {
                Position = new Vector2(-900, -450)
            });

            _TextDisplays.Add(_PointText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"POINTS",
                Position = new Vector2(-840, -40),
                Scale = new Vector2(2f, 2f)
            });
            _TextDisplays.Add(_ShieldText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"INCREASES SHIELD",
                Position = new Vector2(-840, -140),
                Scale = new Vector2(2f, 2f)
            });
            _TextDisplays.Add(_AmmoText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"WEAPON PROBE",
                Position = new Vector2(-840, -240),
                Scale = new Vector2(2f, 2f)
            });
            _TextDisplays.Add(_NukeText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"NUKE",
                Position = new Vector2(-840, -340),
                Scale = new Vector2(2f, 2f)
            });
            _TextDisplays.Add(_HeartText = new TextDisplay(Game.UiCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/KenPixel/KenPixel.png"), Constants.Font.Map, 15, 15, "KenPixel")
            {
                Text = $"EXTRA LIFE",
                Position = new Vector2(-840, -440),
                Scale = new Vector2(2f, 2f)
            });
        }

        /// <summary>
        /// Shows the tutrorial elements
        /// </summary>
        public void Show()
        {
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Visible = true;
            }

            foreach (TextDisplay display in _TextDisplays)
            {
                display.Visible = true;
            }
        }

        /// <summary>
        /// Hides the tutorial elements
        /// </summary>
        public void Hide()
        {
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Visible = false;
            }

            foreach (TextDisplay display in _TextDisplays)
            {
                display.Visible = false;
            }
        }

        #region Overrides of GameObject

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Dispose();
            }
            _Sprites.Clear();
            foreach (TextDisplay display in _TextDisplays)
            {
                display.Dispose();
            }
            _TextDisplays.Clear();
        }

        #endregion
    }
}
