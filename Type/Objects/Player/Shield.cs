using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Audio;
using AmosShared.Graphics;
using Type.Base;
using Type.Data;
using Type.Interfaces;

namespace Type.Objects.Player
{
    public class Shield : GameObject, IShield
    {
        /// <summary> Graphic for level 1 shield </summary>
        private readonly Sprite _Level1Sprite;
        /// <summary> Graphic for level 2 shield </summary>
        private readonly Sprite _Level2Sprite;
        /// <summary> Graphic for level 3 shield </summary>
        private readonly Sprite _Level3Sprite;
        /// <summary> List of all the sprites </summary>
        private readonly List<Sprite> _Sprites;
        /// <summary> The shields current level </summary>
        private Int32 _CurrentLevel;
        /// <summary> The shield max level </summary>
        private Int32 _MaxLevel = 3;

        /// <summary> Whether the shield is active </summary>
        public Boolean IsActive { get; private set; }

        /// <summary> Shield that rotects the player for hits, has multiple levels of protection </summary>
        public Shield()
        {
            _Sprites = new List<Sprite>();
            _Sprites.Add(_Level1Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.SHIELD, Texture.GetTexture("Content/Graphics/Shield/shield1.png")));
            _Sprites.Add(_Level2Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.SHIELD, Texture.GetTexture("Content/Graphics/Shield/shield2.png")));
            _Sprites.Add(_Level3Sprite = new Sprite(Game.MainCanvas, Constants.ZOrders.SHIELD, Texture.GetTexture("Content/Graphics/Shield/shield3.png")));
            foreach (Sprite sprite in _Sprites) { sprite.Offset = sprite.Size / 2; }

            _CurrentLevel = 0;
        }

        /// <summary>
        /// Increases the shields level
        /// </summary>
        public void Increase()
        {
            if (_CurrentLevel >= _MaxLevel) return;
            _CurrentLevel++;
            IsActive = _CurrentLevel > 0;
            GameStats.Instance.ShieldsCreated++;
            UpdateSprites();
            new AudioPlayer("Content/Audio/shield_on.wav", false, AudioManager.Category.EFFECT, 1);
        }

        private void UpdateSprites()
        {
            switch (_CurrentLevel)
            {
                case 0:
                    {
                        ResetSprites();
                        break;
                    }
                case 1:
                    {
                        ResetSprites();
                        _Level1Sprite.Visible = true;
                        break;
                    }
                case 2:
                    {
                        ResetSprites();
                        _Level2Sprite.Visible = true;
                        break;
                    }
                case 3:
                    {
                        ResetSprites();
                        _Level3Sprite.Visible = true;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetSprites()
        {
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Visible = false;
            }
        }

        /// <summary>
        /// Decreases the shields level
        /// </summary>
        public void Decrease()
        {
            if (_CurrentLevel < 0) return;
            _CurrentLevel--;
            IsActive = _CurrentLevel > 0;
            UpdateSprites();
            new AudioPlayer("Content/Audio/shield_off.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <summary>
        /// Completely deactivates the shield
        /// </summary>
        public void Disable()
        {
            _CurrentLevel = 0;
            UpdateSprites();
        }

        /// <summary>
        /// Updates the shield position
        /// </summary>
        /// <param name="position"></param>
        public void UpdatePosition(Vector2 position)
        {
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Position = position;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            base.Dispose();
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Dispose();
            }
        }
    }
}
