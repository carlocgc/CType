using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Interfaces;
using OpenTK;


namespace Type.Interfaces.Probe
{
    public interface IProbeController : IUpdatable
    {
        Boolean Shoot { get; set; }

        void AddProbe(Int32 id);

        void RemoveAll();

        void UpdatePosition(Vector2 position);
    }
}
