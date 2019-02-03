using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;

namespace Type.Objects.World
{
    public class ScrollingBackground : GameObject
    {
        private readonly List<Sprite> _Sprites = new List<Sprite>();

        private readonly Single _Speed;

        private readonly String _AssetPath;

        private Boolean _Updating;

        public ScrollingBackground(Single speed, String assetPath) : base()
        {
            _Speed = speed;
            _AssetPath = assetPath;

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture(_AssetPath))
            {
                Position = new Vector2(-960, -540),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND,
            });

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture(_AssetPath))
            {
                Position = new Vector2(_Sprites[0].Position.X + _Sprites[0].Width / 2, -540),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND,
            });
        }

        public void Start()
        {
            _Updating = true;
        }

        private void CreateSprite()
        {
            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture(_AssetPath))
            {
                Position = new Vector2(960, -540),
                Visible = true,
            });
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (!_Updating) return;

            for (Int32 i = 0; i < _Sprites.Count; i++)
            {
                if (i == 0)
                {
                    _Sprites[i].Position = new Vector2(_Sprites[i].Position.X - _Speed * (Single)timeTilUpdate.TotalSeconds, -540);
                }
                else
                {
                    _Sprites[i].Position = new Vector2(_Sprites[i - 1].Position.X + _Sprites[i - 1].Width, -540);
                }

            }

            if (_Sprites[0].Position.X + _Sprites[0].Width < -960)
            {
                _Sprites.Remove(_Sprites[0]);
                CreateSprite();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (Sprite sprite in _Sprites)
            {
                sprite.Dispose();
            }
            _Sprites.Clear();
        }
    }
}
