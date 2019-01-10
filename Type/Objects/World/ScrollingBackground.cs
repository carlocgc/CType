using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using Type.Base;
using Type.Glide;

namespace Type.Objects.World
{
    public class ScrollingBackground : GameObject
    {
        private readonly Tweener _Tweener = new Tweener();

        private readonly List<Sprite> _Sprites = new List<Sprite>();

        private Boolean _Updating;

        private Single _Speed = 5f;

        public ScrollingBackground() : base()
        {
            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/background.png"))
            {
                //Offset = new Vector2(960, 540),
                Position = new Vector2(-960, -540),
                Visible = true,
            });

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/background.png"))
            {
                //Offset = new Vector2(960, 540),
                Position = new Vector2(_Sprites[0].Position.X + _Sprites[0].Width / 2, -540),
                Visible = true,
            });
        }

        public void Start()
        {
            _Updating = true;
        }

        private void CreateSprite()
        {
            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/background.png"))
            {
                //Offset = new Vector2(960, 540),
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
                    _Sprites[i].Position = new Vector2(_Sprites[i].Position.X - _Speed, -540);
                }
                else
                {
                    _Sprites[i].Position = new Vector2(_Sprites[i -1].Position.X + _Sprites[i -1].Width, -540);
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
