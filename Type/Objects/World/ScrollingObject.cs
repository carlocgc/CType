using AmosShared.Graphics;
using AmosShared.Graphics.Drawables;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Base;

namespace Type.Objects.World
{
    public class ScrollingObject : GameObject
    {
        private readonly List<Sprite> _Sprites = new List<Sprite>();

        private readonly Random _Rnd = new Random(Environment.TickCount);

        private readonly Int32 _MinDelay;

        private readonly Int32 _MaxDelay;

        private readonly Int32 _MinSpeed;

        private readonly Int32 _MaxSpeed;

        private Int32 _PreviousIndex;

        private Int32 _CurrentSpriteindex;

        private Int32 _Speed;

        private TimeSpan _TimeSinceLastTransit;

        private TimeSpan _Delay;

        private Boolean _Scrolling;

        private Boolean _Updating;

        public ScrollingObject(Int32 minSpeed, Int32 maxSpeed, String path, Int32 maxAssets, Int32 delayMin, Int32 delayMax, Int32 zOrder)
        {
            _PreviousIndex = 0;
            _MinSpeed = minSpeed;
            _MaxSpeed = maxSpeed;
            _MinDelay = delayMin;
            _MaxDelay = delayMax;

            _Delay = TimeSpan.FromSeconds(_Rnd.Next(_MinDelay, _MaxDelay));

            for (Int32 i = 1; i <= maxAssets; i++)
            {
                _Sprites.Add(new Sprite(Game.MainCanvas, Constants.ZOrders.BACKGROUND, Texture.GetTexture($"{path}{i}.png"))
                {
                    Position = new Vector2(960, GetRandomYPos()),
                    Visible = true,
                    ZOrder = zOrder,
                });
            }
        }

        private Int32 GetRandomYPos()
        {
            return _Rnd.Next(-540, 0);
        }

        public void Start()
        {
            _Updating = true;
        }

        private void BeginScrolling()
        {
            List<Int32> indexes = new List<Int32>();

            for (Int32 i = 0; i < _Sprites.Count; i++)
            {
                indexes.Add(i);
            }
            indexes.Remove(_PreviousIndex);

            _CurrentSpriteindex = indexes[_Rnd.Next(0, indexes.Count)];
            _PreviousIndex = _CurrentSpriteindex;

            _Delay = TimeSpan.FromSeconds(_Rnd.Next(_MinDelay, _MaxDelay));

            _Speed = _Rnd.Next(_MinSpeed, _MaxSpeed);

            _Scrolling = true;
        }

        private void ResetSprite()
        {
            _Sprites[_CurrentSpriteindex].Position = new Vector2(960, GetRandomYPos());
        }

        public override void Update(TimeSpan timeTilUpdate)
        {
            base.Update(timeTilUpdate);

            if (!_Updating) return;

            if (_Scrolling)
            {
                _Sprites[_CurrentSpriteindex].Position = new Vector2(_Sprites[_CurrentSpriteindex].Position.X - _Speed * (Single)timeTilUpdate.TotalSeconds,
                    _Sprites[_CurrentSpriteindex].Position.Y);

                if (_Sprites[_CurrentSpriteindex].Position.X + _Sprites[_CurrentSpriteindex].Width < -960)
                {
                    ResetSprite();
                    _Scrolling = false;
                }
            }
            else
            {
                _TimeSinceLastTransit += timeTilUpdate;
                if (_TimeSinceLastTransit >= _Delay)
                {
                    _TimeSinceLastTransit = TimeSpan.Zero;
                    BeginScrolling();
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
