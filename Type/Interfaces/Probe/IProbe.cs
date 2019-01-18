using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Type.Interfaces.Probe
{
    public interface IProbe
    {
        Boolean Shoot { get; set; }

        void UpdatePosition(Vector2 position);
    }
}
