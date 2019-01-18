using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces.Control
{
    public interface IAnalogListener
    {
        void UpdatePosition(Vector2 direction, Single strength);
    }
}
