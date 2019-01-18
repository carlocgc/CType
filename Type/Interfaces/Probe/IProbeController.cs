using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace Type.Interfaces.Probe
{
    public interface IProbeController
    {
        void AddProbe();

        void Fire();

        void RemoveAll();

        void UpdatePosition(Vector2 position);
    }
}
