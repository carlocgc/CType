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
    /// **This exists because the engine has eight audio sources and no opinion about who gets
    /// them.** <see cref="AudioManager"/> hands out one of eight, and when they are all busy the
    /// <see cref="AudioPlayer"/> constructor gives up and writes a line to the console. Whichever
    /// sound asked last loses, so in a firefight the thing that goes quiet is whatever happened
    /// to be most recent rather than whatever mattered least.
    /// <para>
    /// **The measurement, at level 11 with the ship auto-firing, over 150 seconds:** with the old
    /// per-enemy rate limits in place the engine dropped **1** sound; with them removed it dropped
    /// **128**. The limits were load bearing, which is why they could not simply be deleted, and
    /// 128 is the pressure this controller has to absorb in their place. See G4 in ROADMAP.md.
    /// </para>
    /// <para>
    /// **Why this replaces seven copies of a per-enemy timer.** Each enemy used to hold its own
    /// `_IsSoundPlaying` flag and allow itself one hit sound every 0.2 seconds. That bounds one
    /// enemy and nothing else: twenty of them on screen is still up to a hundred requests a
    /// second into eight sources. The limit has to be global to mean anything, and a global limit
    /// needs somewhere global to live.
    /// </para>
    /// <para>
    /// **Timed against the wall clock, not game time.** The same trap G3's shake and hit stop hit,
    /// and I8's rumble before them: the update a game object receives has already been scaled by
    /// <see cref="TimeScaleController"/>, so a limit measured in game time would stretch to
    /// several times its length during a hit stop — exactly when the most sounds are being asked
    /// for. This is the fourth time game time has been the wrong clock for a feedback effect.
    /// </para>
    /// <para>
    /// **The budget is advisory, and deliberately so.** Clip lengths come from the audio data, so
    /// the controller knows roughly how long each source stays busy, but it cannot see sources the
    /// engine gave to anything else — music, or a caller that has not been routed through here
    /// yet. It also over-admits after the game has been left unfocused, because
    /// <see cref="AmosShared.Base.BaseGame.Pause"/> pauses the sources without stopping the wall
    /// clock. Both cases fail into the engine's own behaviour, which is to drop the sound, so
    /// being wrong here costs a dropped effect rather than a crash.
    /// </para>
    /// </remarks>
    public class AudioController
    {
        /// <summary> The instance of the audio controller </summary>
        private static AudioController _Instance;

        /// <summary>
        /// How many of the engine's eight sources effects are allowed to hold at once.
        /// The remainder is what keeps music off the failure path described in G4: a track
        /// started while every source is busy comes back half constructed, and stopping it later
        /// throws inside the engine.
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
