using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Audio;

namespace Type.UI
{
    /// <summary>
    /// Visual object that displays the remaning player lives
    /// </summary>
    public class LifeMeter : IDisposable
    {
        /// <summary> Sprites that show the current amount of lifes </summary>
        private readonly List<Sprite> _LifeSprites;
        /// <summary> Total lives the player can hold </summary>
        private Int32 _MaxLives;
        /// <summary> Amount of times the player can die before game over </summary>
        public Int32 PlayerLives { get; private set; }

        /// <summary>
        /// Shows the player lives
        /// </summary>
        /// <param name="playerId"> the type of player ship selected </param>
        public LifeMeter(Int32 playerId)
        {
            PlayerLives = 3;

            String type;

            switch (playerId)
            {
                case 0:
                    {
                        type = "alpha";
                        break;
                    }
                case 1:
                    {
                        type = "beta";
                        break;
                    }
                case 2:
                    {
                        type = "gamma";
                        break;
                    }
                case 3:
                    {
                        PlayerLives = 1;
                        type = "omega";
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("Player ship type does not exist");
            }

            _LifeSprites = new List<Sprite>
            {
                new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture($"Content/Graphics/UI/player-{type}-lifeicon.png")) {Position = new Vector2(-900, 400), Visible = false,},
                new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture($"Content/Graphics/UI/player-{type}-lifeicon.png")) {Position = new Vector2(-836, 400), Visible = false,},
                new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture($"Content/Graphics/UI/player-{type}-lifeicon.png")) {Position = new Vector2(-772, 400), Visible = false,},
                new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture($"Content/Graphics/UI/player-{type}-lifeicon.png")) {Position = new Vector2(-708, 400), Visible = false,},
                new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture($"Content/Graphics/UI/player-{type}-lifeicon.png")) {Position = new Vector2(-644, 400), Visible = false,}
            };

            UpdateSprites();
        }

        /// <summary>
        /// Decrements the current lifes and updates the sprites visibility
        /// </summary>
        public void LoseLife()
        {
            PlayerLives--;
            UpdateSprites();
            new AudioPlayer("Content/Audio/death.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <summary>
        /// Increments the current lifes and updates the sprites visibility
        /// </summary>
        public void AddLife()
        {
            if (PlayerLives >= 5) return;
            PlayerLives++;
            UpdateSprites();
            new AudioPlayer("Content/Audio/lifeup.wav", false, AudioManager.Category.EFFECT, 1);
        }

        /// <summary>
        /// Makes the sprites visibility match the current number of lifes
        /// </summary>
        private void UpdateSprites()
        {
            for (Int32 i = 0; i < _LifeSprites.Count; i++)
            {
                if (i < PlayerLives)
                {
                    _LifeSprites[i].Visible = true;
                }
                else
                {
                    _LifeSprites[i].Visible = false;
                }
            }
        }

        public void Dispose()
        {
            foreach (Sprite lifeSprite in _LifeSprites) lifeSprite.Dispose();
            _LifeSprites.Clear();
        }
    }
}
