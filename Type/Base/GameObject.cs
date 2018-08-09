using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using AmosShared.Graphics.Drawables;
using AmosShared.Interfaces;
using OpenTK;

namespace Type.Base
{
    public abstract class GameObject : IUpdatable, IPositionable
    {
        /// <summary> Whether the object is </summary>
        public Boolean IsDisposed { get; set; }
        /// <summary> Whether the object is updated </summary>
        public Boolean UpdateEnabled { get; protected set; }
        /// <summary> Sprite for the game object </summary>
        private Sprite _Sprite;
        /// <summary> Position of the object </summary>
        private Vector2 _Position;

        protected GameObject(Boolean updateEnabled = true)
        {
            UpdateEnabled = updateEnabled;
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary> Position of the object </summary>
        public virtual Vector2 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                _Sprite.Position = value;
            }
        }

        /// <summary>
        /// Returns the sprite for this object
        /// </summary>
        public Sprite GetSprite()
        {
            return _Sprite;
        }

        /// <summary>
        /// Returns the rectangle of this game object
        /// </summary>
        public Vector4 GetRect()
        {
            return new Vector4(_Sprite.Position.X, _Sprite.Position.Y, _Sprite.Width, _Sprite.Height);
        }

        /// <summary>
        /// Whether the object will be updated
        /// </summary>
        public Boolean CanUpdate()
        {
            return UpdateEnabled;
        }

        /// <summary>
        /// Updates the sprites position with this game object 
        /// </summary>
        protected void AddSprite(Sprite sprite)
        {
            _Sprite = sprite;
            _Sprite.Position = Position;
        }

        public virtual void Update(TimeSpan timeTilUpdate)
        {

        }

        public virtual void Dispose()
        {
            _Sprite?.Dispose();
            _Sprite = null;
            UpdateEnabled = false;
            UpdateManager.Instance.RemoveUpdatable(this);
            IsDisposed = true;
        }
    }
}
