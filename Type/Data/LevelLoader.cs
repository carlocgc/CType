﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;

namespace Type.Data
{
    /// <summary>
    /// Object that will create wave data objects from txt file data
    /// </summary>
    public class LevelLoader
    {
        /// <summary>
        /// Returns a list of wave data objects
        /// </summary>
        /// <param name="level"> The level to get the wave data for </param>
        /// <returns></returns>
        public List<WaveData> GetWaveData(Int32 level)
        {
            String filepath = $"/Content/Level/level-{level}.txt";
            StreamReader sr = new StreamReader(filepath);
            List<String> lines = new List<String>();
            List<WaveData> _Waves = new List<WaveData>();

            while (!sr.EndOfStream)
            {
                lines.Add(sr.ReadLine());
            }

            foreach (String line in lines)
            {
                String[] parts = line.Split(',');
                List<Vector2> positions = new List<Vector2>();

                Single.TryParse(parts[0], out Single interval);
                Int32.TryParse(parts[1], out Int32 shiptype);

                for (int i = 2; i < parts.Length; i++)
                {
                    positions.Add(new Vector2(1100, Int32.Parse(parts[i])));
                }

                _Waves.Add(new WaveData(TimeSpan.FromSeconds(interval), shiptype, positions.ToArray()));
            }
            sr.Close();
            return _Waves;
        }
    }
}
