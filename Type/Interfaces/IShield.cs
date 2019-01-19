using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Interfaces;
using OpenTK;

namespace Type.Interfaces
{
    public interface IShield : IPositionable

    {
    Boolean IsActive { get; }

    Vector2 Position { get; set; }

    void Activate();

    void Deactivate();
    }
}
