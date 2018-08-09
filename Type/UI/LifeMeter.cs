using System;
using System.Collections.Generic;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;

namespace Type.UI
{
    /// <summary>
    /// Visual object that displays the remaning player lives
    /// </summary>
    public class LifeMeter : GameObject
    {
        /// <summary> Total lives the player can hold </summary>
        private Int32 _MaxLives;
        /// <summary> Sprites that show the current amount of lifes </summary>
        private List<Sprite> _LifeSprites;

        /// <summary> Amount of times the player can die before game over </summary>
        public Int32 PlayerLives { get; private set; }

        public LifeMeter()
        {
            PlayerLives = 3;
            _LifeSprites = new List<Sprite>();
            _LifeSprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/lifeicon.png"))
            {
                Position = new Vector2(-900, -500),
                Visible = false,
            });            
            _LifeSprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/lifeicon.png"))
            {
                Position = new Vector2(-836, -500),
                Visible = false,
            });
            _LifeSprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/lifeicon.png"))
            {
                Position = new Vector2(-772, -500),
                Visible = false,
            });
            _LifeSprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/lifeicon.png"))
            {
                Position = new Vector2(-708, -500),
                Visible = false,
            });
            _LifeSprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.UI, Texture.GetTexture("Content/Graphics/lifeicon.png"))
            {
                Position = new Vector2(-644, -500),
                Visible = false,
            });

            UpdateSprites();
        }

        /// <summary>
        /// Decrements the current lifes and updates the sprites visibility
        /// </summary>
        public void LoseLife()
        {
            PlayerLives--;
            UpdateSprites();
        }

        /// <summary>
        /// Increments the current lifes and updates the sprites visibility
        /// </summary>
        public void AddLife()
        {
            PlayerLives++;
            UpdateSprites();
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

        public override void Dispose()
        {
            base.Dispose();
            foreach (Sprite lifeSprite in _LifeSprites)
            {
                lifeSprite.Dispose();
            }
            _LifeSprites.Clear();
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);
        }
    }
}
