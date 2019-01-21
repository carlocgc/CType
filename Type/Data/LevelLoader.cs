using AmosShared.Base;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Type.Data
{
    /// <summary>
    /// Object that will create wave data objects from txt file data
    /// </summary>
    public static class LevelLoader
    {
        /// <summary>
        /// Returns a list of wave data objects
        /// </summary>
        /// <param name="level"> The level to get the wave data for </param>
        /// <returns></returns>
        public static List<WaveData> GetWaveData(Int32 level)
        {
            // Get level data as a string
            String filepath = $"Content/Level/level-{level}.txt";
            String data = DataLoader.ReadTextFile(filepath, false);

            List<String> waveStrings = new List<String>();
            List<WaveData> _Waves = new List<WaveData>();

            // Split string into wave strings
            waveStrings = data.Split(':').ToList();

            // Process each wave
            foreach (String line in waveStrings)
            {
                // Split wave string into each enemy data section
                String[] enemyStrings = line.Split(',');
                List<Vector2> positions = new List<Vector2>();
                List<Int32> types = new List<Int32>();
                List<Single> delays = new List<Single>();

                // For each enemy in the wave
                foreach (String es in enemyStrings)
                {
                    // Split enemy data section into : type, Y position, spawn delay
                    String[] shipdata = es.Split('_');
                    Int32.TryParse(shipdata[0], out Int32 shiptype);
                    Single.TryParse(shipdata[1], out Single yPos);
                    Single.TryParse(shipdata[2], out Single delay);

                    // Add each parsed string to its respective list
                    types.Add(shiptype);
                    positions.Add(new Vector2(1100, yPos));
                    delays.Add(delay);
                }

                // Create the delay time spans
                List<TimeSpan> delaySpans = new List<TimeSpan>();
                foreach (Single delay in delays)
                {
                    delaySpans.Add(TimeSpan.FromSeconds(delay));
                }

                // Create the wave data
                _Waves.Add(new WaveData(delaySpans.ToArray(), types.ToArray(), positions.ToArray()));
            }

            return _Waves;
        }
    }
}
