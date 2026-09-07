using AmosShared.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Type.Controllers
{
    /// <summary>
    /// The single place a sound effect is asked for, and the only thing that decides whether it
    /// actually plays.
    /// </summary>
    /// <remarks>
    /// The engine has a limited pool of audio sources and drops whatever asks once they are all
    /// busy, so without a limit the sound that goes quiet in a firefight is whichever asked last.
    /// This holds a global minimum interval per sound and a ceiling on how many effects run at
    /// once; the numbers live in <see cref="Data.Sounds"/>.
    /// <para>
    /// Intervals are wall clock, not game time. The update a game object receives has already been
    /// scaled by <see cref="TimeScaleController"/>, so a limit in game time would stretch during a
    /// hit stop, which is exactly when the most sounds are asked for.
    /// </para>
    /// <para>
    /// The budget is advisory: it cannot see sources the engine gave to anything else, and it
    /// over-admits after the game has been left unfocused, because pausing stops the sources
    /// without stopping the clock. Both cases fail into the engine dropping the sound rather than
    /// into anything worse. See G4 in ROADMAP.md for the measurements behind the numbers.
    /// </para>
    /// </remarks>
    public class AudioController
    {
        /// <summary> The instance of the audio controller </summary>
        private static AudioController _Instance;

        /// <summary>
        /// How many effects may hold a source at once, leaving the rest of the engine's pool for
        /// music. It refuses about 0.3% of requests, so it is a safety valve rather than the thing
        /// shaping what is heard - the intervals do that.
        /// </summary>
        private const Int32 EFFECT_BUDGET = 6;

        /// <summary> Counts wall clock time, for the reason given on the class </summary>
        private readonly Stopwatch _Clock = Stopwatch.StartNew();
        /// <summary> When each sound was last allowed through, keyed by file </summary>
        private readonly Dictionary<String, TimeSpan> _LastPlayed = new Dictionary<String, TimeSpan>();
        /// <summary> How long each sound occupies a source, keyed by file </summary>
        private readonly Dictionary<String, TimeSpan> _Durations = new Dictionary<String, TimeSpan>();
        /// <summary> When each effect this controller started is expected to finish </summary>
        private readonly List<TimeSpan> _InFlight = new List<TimeSpan>();

        /// <summary> The instance of the audio controller </summary>
        public static AudioController Instance => _Instance ?? (_Instance = new AudioController());

        /// <summary> Creates the audio controller </summary>
        private AudioController()
        {
        }

        /// <summary>
        /// Plays a sound effect, unless the same sound played too recently or every source the
        /// budget allows is already busy.
        /// </summary>
        /// <param name="file"> The output path of the sound, as <see cref="AudioPlayer"/> wants it </param>
        /// <param name="volume"> The volume to play it at, before the category and master volumes </param>
        /// <param name="minimumInterval"> The shortest wall clock gap allowed between two plays of this sound </param>
        public void Play(String file, Single volume, TimeSpan minimumInterval)
        {
            TimeSpan now = _Clock.Elapsed;

            RetireFinished(now);

            if (_LastPlayed.TryGetValue(file, out TimeSpan last) && now - last < minimumInterval) return;
            if (_InFlight.Count >= EFFECT_BUDGET) return;

            _LastPlayed[file] = now;
            _InFlight.Add(now + DurationOf(file));

            new AudioPlayer(file, false, AudioManager.Category.EFFECT, volume);
        }

        /// <summary>
        /// Forgets the effects that have finished, so <see cref="EFFECT_BUDGET"/> is counted
        /// against what is still playing rather than everything ever started.
        /// </summary>
        /// <param name="now"> The current wall clock reading </param>
        private void RetireFinished(TimeSpan now)
        {
            for (Int32 i = _InFlight.Count - 1; i >= 0; i--)
            {
                if (_InFlight[i] > now) continue;
                _InFlight.RemoveAt(i);
            }
        }

        /// <summary>
        /// How long the given sound plays for, worked out from the audio data the engine has
        /// already decoded and cached, and remembered so the arithmetic happens once per sound.
        /// </summary>
        /// <param name="file"> The output path of the sound </param>
        /// <returns> The length of the clip </returns>
        private TimeSpan DurationOf(String file)
        {
            if (_Durations.TryGetValue(file, out TimeSpan cached)) return cached;

            AudioData data = AudioData.GetData(file);
            Int32 bytesPerSecond = data.SampleRate * data.NumChannels * (data.BitsPerSample / 8);
            TimeSpan duration = bytesPerSecond > 0
                ? TimeSpan.FromSeconds((Double)data.Data.Length / bytesPerSecond)
                : TimeSpan.Zero;

            _Durations[file] = duration;
            return duration;
        }
    }
}
