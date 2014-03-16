using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OsuPlayer.Core.Osu
{
    public class Timing
    {
        public double MillisecondsPerBeat { get; private set; }
        public long Offset { get; private set; }

        public static List<Timing> FromSong(Song song)
        {
            string[] lines = File.ReadAllLines(song.Path);
            bool start = false;
            List<Timing> times = new List<Timing>();
            foreach (string line in lines)
            {
                if (start && line.StartsWith("[")) break;
                if (String.IsNullOrEmpty(line)) continue;
                if (line == "[TimingPoints]")
                {
                    start = true;
                    continue;
                }
                else if (start)
                {
                    long offset = 0;
                    long.TryParse(line.Split(',')[0], out offset);
                    double mpb = 1000;
                    double.TryParse(line.Split(',')[1], out mpb);
                    if (mpb < 0)
                    {
                        for (int i = times.Count - 1; i >= 0; i--)
                        {
                            mpb = times[i].MillisecondsPerBeat;
                            if (mpb > 0) break;
                        }
                    }

                    Timing time = new Timing();
                    time.Offset = offset;
                    time.MillisecondsPerBeat = mpb;

                    times.Add(time);
                }
            }

            return times;
        }

        public double CalculateBeats(long milliDuration)
        {
            return (double)milliDuration / MillisecondsPerBeat;
        }
    }
}
