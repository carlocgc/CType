using AmosShared.Base;
using OpenTK;
using System;
using System.Collections.Generic;
using Type.Data;
using Type.Interfaces.Probe;
using Type.Objects.Probes;

namespace Type.Controllers
{
    /// <summary>
    /// Manages the probes attached to the player ship
    /// </summary>
    public class ProbeController : IProbeController
    {
        /// <summary> List of all the probes </summary>
        private readonly List<IProbe> _Probes;
        /// <summary> The position the probes orbit </summary>
        private Vector2 _OrbitPosition;
        /// <summary> Total probes </summary>
        private Int32 _ProbeCount;
        /// <summary> Whether the probes are shooting </summary>
        private Boolean _Shoot;

        /// <summary> Whether the probes are shooting </summary>
        public Boolean Shoot
        {
            get => _Shoot;
            set
            {
                _Shoot = value;
                foreach (IProbe probe in _Probes)
                {
                    probe.Shoot = _Shoot;
                }
            }
        }

        /// <inheritdoc />
        public Boolean IsDisposed { get; set; }

        public ProbeController()
        {
            _Probes = new List<IProbe>();
            UpdateManager.Instance.AddUpdatable(this);
        }

        /// <summary>
        /// Creates <see cref="_ProbeCount"/> probes
        /// </summary>
        /// <param name="id"></param>
        public void AddProbe(Int32 id)
        {
            _ProbeCount++;
            Single spaceBetweenProbes = (Single)Math.PI * 2 / _ProbeCount;
            GameStats.Instance.ProbesCreated++;

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
                        // Recreate the list of probes using the current count
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

        /// <summary>
        /// Destroys all the probes
        /// </summary>
        public void RemoveAll()
        {
            foreach (IProbe probe in _Probes)
            {
                probe.Dispose();
            }
            _Probes.Clear();
            _ProbeCount = 0;
        }

        /// <summary>
        /// Updates the orbit position of all the probes
        /// </summary>
        /// <param name="position"></param>
        public void UpdatePosition(Vector2 position)
        {
            _OrbitPosition = position;

            foreach (IProbe probe in _Probes)
            {
                probe.UpdatePosition(_OrbitPosition);
            }
        }

        public void Update(TimeSpan timeTilUpdate)
        {
        }

        public Boolean CanUpdate()
        {
            return true;
        }

        public void Dispose()
        {
            foreach (IProbe probe in _Probes)
            {
                probe.Dispose();
            }
            UpdateManager.Instance.RemoveUpdatable(this);
        }
    }
}
