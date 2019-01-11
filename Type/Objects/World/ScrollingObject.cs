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
    public class ScrollingObject : GameObject
    {
        private readonly List<Sprite> _Sprites = new List<Sprite>();

        private readonly Random _Rnd = new Random(Environment.TickCount);

        private TimeSpan _TimeSinceLastTransit;

        private TimeSpan _Delay;

        private Int32 _PreviousIndex;

        private Int32 _CurrentSpriteindex;

        private Single _MaxDelay;

        private Single _MinDelay;

        private Boolean _Updating;

        private Single _Speed;

        public ScrollingObject() : base()
        {
            _PreviousIndex = 0;

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/planet-1.png"))
            {
                Position = new Vector2(960, GetRandomYPos()),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND_OBJECT,
            });

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/planet-2.png"))
            {
                Position = new Vector2(960, GetRandomYPos()),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND_OBJECT,
            });

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/planet-3.png"))
            {
                Position = new Vector2(960, GetRandomYPos()),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND_OBJECT,
            });

            _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture("Content/Graphics/cluster.png"))
            {
                Position = new Vector2(960, GetRandomYPos()),
                Visible = true,
                ZOrder = Constants.ZOrders.BACKGROUND_OBJECT,
            });
        }

        private Int32 GetRandomYPos()
        {
            return _Rnd.Next(-400, 300);
        }

        public void Start()
        {
            List<Int32> indexes = new List<Int32>();

            for (Int32 i = 0; i < _Sprites.Count; i++)
            {
                indexes.Add(i);
            }
            indexes.Remove(_PreviousIndex);

            _CurrentSpriteindex = indexes[_Rnd.Next(0, indexes.Count)];
            _PreviousIndex = _CurrentSpriteindex;

            _Delay = TimeSpan.FromSeconds(_Rnd.Next(5, 6));

            _Speed = _Rnd.Next(280, 350);

            _Updating = true;
        }

        private void ResetSprite()
        {
            _Sprites[_CurrentSpriteindex].Position = new Vector2(960, GetRandomYPos());
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (_Updating)
            {
                _Sprites[_CurrentSpriteindex].Position = new Vector2(_Sprites[_CurrentSpriteindex].Position.X - _Speed * (Single)timeTilUpdate.TotalSeconds,
                    _Sprites[_CurrentSpriteindex].Position.Y);

                if (_Sprites[_CurrentSpriteindex].Position.X + _Sprites[_CurrentSpriteindex].Width < -960)
                {
                    ResetSprite();
                    _Updating = false;
                }
            }
            else
            {
                _TimeSinceLastTransit += timeTilUpdate;
                if (_TimeSinceLastTransit >= _Delay)
                {
                    _TimeSinceLastTransit = TimeSpan.Zero;
                    Start();
                }
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
