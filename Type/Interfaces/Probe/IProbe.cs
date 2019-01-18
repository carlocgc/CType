using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces.Probe
{
    public interface IProbe
    {
        Int32 ID { get; }

        void Fire();

        void UpdatePosition(Vector2 position);
    }
}
