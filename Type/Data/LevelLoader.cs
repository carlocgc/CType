using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;

namespace Type.Data
{
    public class LevelLoader
    {
        public List<WaveData> GetWaveData(Int32 level)
        {
            String filepath = $"Content/Level/level-{level}.txt";
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
                Single interval;
                Int32 shiptype;
                List<Vector2> positions = new List<Vector2>();

                Single.TryParse(parts[0], out interval);
                Int32.TryParse(parts[1], out shiptype);

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
