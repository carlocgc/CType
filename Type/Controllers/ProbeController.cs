using System;
using System.Collections.Generic;
using System.Text;
using AmosShared.Base;
using OpenTK;
using Type.Interfaces.Probe;
using Type.Objects.Probes;

namespace Type.Controllers
{
    public class ProbeController : IProbeController
    {
        private readonly List<IProbe> _Probes;

        public Boolean IsDisposed { get; set; }

        private Vector2 _OrbitPosition;

        private Int32 _ProbeCount;

        public ProbeController()
        {
            _Probes = new List<IProbe>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        public Boolean Shoot { get; set; }

        public void AddProbe(Int32 id)
        {
            _ProbeCount++;
            Single spaceBetweenProbes = (Single)Math.PI * 2 / _ProbeCount;

            // First clear all the probes
            foreach (IProbe probe in _Probes)
            {
                probe.Dispose();
            }
            _Probes.Clear();

            switch (id)
            {
                case 0:
                    {
                        for (Int32 i = 0; i < _ProbeCount; i++)
                        {
                            _Probes.Add(new LaserProbe(_OrbitPosition, i * spaceBetweenProbes));
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveAll()
        {
            foreach (IProbe probe in _Probes)
            {
                probe.Dispose();
            }
            _Probes.Clear();
            _ProbeCount = 0;
        }

        public void UpdatePosition(Vector2 position)
        {
            _OrbitPosition = position;

            foreach (IProbe probe in _Probes)
            {
                probe.UpdatePosition(_OrbitPosition);
            }
        }

        public void Dispose()
        {
            foreach (IProbe probe in _Probes)
            {
                probe.Dispose();
            }
            UpdateManager.Instance.RemoveUpdatable(this);
        }

        public void Update(TimeSpan timeTilUpdate)
        {
            foreach (IProbe probe in _Probes)
            {
                probe.Shoot = Shoot;
            }
        }

        public Boolean CanUpdate()
        {
            return true;
        }
    }
}
