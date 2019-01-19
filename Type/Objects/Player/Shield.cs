using AmosShared.Graphics.Drawables;
using System;
using Type.Base;
using Type.Interfaces;

namespace Type.Objects.Player
{
    public class Shield : GameObject, IShield
    {
        private Sprite _Sprite;

        public Boolean IsActive { get; private set; }

        public Shield()
        {
            //_Sprite = new Sprite(0);
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {

        }
    }
}
