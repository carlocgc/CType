using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Graphics.Drawables;
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
            _Sprite = new Sprite();
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
