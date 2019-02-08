using AmosShared.Base;
using OpenTK;
using System;
using System.Collections.Generic;
using AmosShared.Audio;
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
        /// <summary> Maximum amount of probes the controoller can have </summary>
        private readonly Int32 _MaxProbes = 6;
        /// <summary> The position the probes orbit </summary>
        private Vector2 _OrbitPosition;
        /// <summary> Total probes </summary>
        private Int32 _ProbeCount;
        /// <summary> Whether the probes are shooting </summary>
        private Boolean _AutoFire;

        /// <summary>
        /// Current probe count
        /// </summary>
        public Int32 CurrentProbes => _ProbeCount;

        /// <inheritdoc />
        public Boolean WeaponsAtMax => _Probes.Count == _MaxProbes;

        /// <summary> Whether the probes are shooting </summary>
        public Boolean Shoot
        {
            get => _AutoFire;
            set
            {
                _AutoFire = value;
                foreach (IProbe probe in _Probes)
                {
                    probe.AutoFire = _AutoFire;
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

            new AudioPlayer("Content/Audio/upgrade1.wav", false, AudioManager.Category.EFFECT, 1);
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
        /// <param name="deltaTime"></param>
        public void UpdatePosition(Vector2 position, Single deltaTime)
        {
            _OrbitPosition = position;

            foreach (IProbe probe in _Probes)
            {
                probe.UpdatePosition(_OrbitPosition, deltaTime);
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
