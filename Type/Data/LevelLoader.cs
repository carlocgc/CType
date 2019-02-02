using AmosShared.Base;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using Boolean = System.Boolean;
using String = System.String;

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

            String cleanData = data.ToLower().Replace("\r\n", "");

            // Split data into wave strings
            waveStrings = cleanData.Split('_').ToList();

            // Temporary data store wave data object
            List<Single> positions = new List<Single>();
            List<Int32> types = new List<Int32>();
            List<Single> delays = new List<Single>();
            List<Int32> moveTypes = new List<Int32>();
            List<Single> xDirections = new List<Single>();
            List<Single> yDirections = new List<Single>();
            List<Single> speeds = new List<Single>();

            foreach (String waveString in waveStrings)
            {
                String[] waveData = waveString.Split(':');

                String[] enemyStrings = waveData[1].Split(';');

                foreach (String enemyString in enemyStrings)
                {
                    String[] enemieParts = enemyString.Split('|');

                    foreach (String part in enemieParts)
                    {
                        String[] partSplit = part.Split('=');

                        switch (partSplit[0])
                        {
                            case "type":
                                {
                                    types.Add(Int32.Parse(partSplit[1]));
                                    break;
                                }
                            case "ypos":
                                {
                                    positions.Add(Int32.Parse(partSplit[1]));
                                    break;
                                }
                            case "delay":
                                {
                                    delays.Add(Single.Parse(partSplit[1]));
                                    break;
                                }
                            case "movetype":
                                {
                                    moveTypes.Add(Int32.Parse(partSplit[1]));
                                    break;
                                }
                            case "xdir":
                                {
                                    xDirections.Add(Single.Parse(partSplit[1]));
                                    break;
                                }
                            case "ydir":
                                {
                                    yDirections.Add(Single.Parse(partSplit[1]));
                                    break;
                                }
                            case "speed":
                                {
                                    speeds.Add(Single.Parse(partSplit[1]));
                                    break;
                                }
                            default:
                                {
                                    throw new ArgumentOutOfRangeException("Enemy component does not exist");
                                }
                        }
                    }
                }

                // Create the delay time spans
                List<TimeSpan> delaySpans = new List<TimeSpan>();
                foreach (Single delay in delays)
                {
                    delaySpans.Add(TimeSpan.FromSeconds(delay));
                }

                List<Vector2> directions = new List<Vector2>();
                Int32 index = 0;
                foreach (var xDirection in xDirections)
                {
                    directions.Add(new Vector2(xDirection, yDirections[index]));
                    index++;
                }

                // Create the wave data
                _Waves.Add(new WaveData(delaySpans.ToArray(), types.ToArray(), positions.ToArray(), moveTypes.ToArray(), directions.ToArray(), speeds.ToArray()));

                delaySpans.Clear();
                types.Clear();
                positions.Clear();
                moveTypes.Clear();
                xDirections.Clear();
                yDirections.Clear();
                speeds.Clear();
            }

            return _Waves;
        }
    }
}
