using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OsuPlayer.Core
{
    public class Beat
    {
        public long Time { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public static Queue<Beat> FromSong(Song song)
        {
            Queue<Beat> beats = new Queue<Beat>();
            string[] lines = File.ReadAllLines(song.Path);
            bool start = false;
            foreach (string line in lines)
            {
                if (line == "[HitObjects]")
                {
                    start = true;
                    continue;
                }
                else if (start)
                {
                    Beat beat = new Beat();
                    long time = 0;
                    long.TryParse(line.Split(',')[2], out time);
                    beat.Time = time;

                    beats.Enqueue(beat);
                }
            }

            return beats;
        }
    }
}
