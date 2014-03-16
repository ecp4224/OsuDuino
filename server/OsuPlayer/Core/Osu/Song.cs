using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsuPlayer.Core.Osu
{
    public class Song
    {
        public string Path;
        public string Name;
        public double PixelSpeed
        {
            get
            {
                return 100 * TickRate;
            }
        }
        public double TickRate { get; private set; }
        public Queue<Beat> Beats;
        private List<Timing> times;
        private int _timeOffset = 0;
        public Timing CurrentTimer
        {
            get;
            private set;
        }

        public void LoadTimers()
        {
            times = Timing.FromSong(this);
        }

        public void UpdateTimer(long SongOffset)
        {
            for (int i = _timeOffset; i < times.Count; i++)
            {
                if (SongOffset >= times[i].Offset)
                {
                    CurrentTimer = times[i];
                    _timeOffset = i;
                }
                else break;
            }
        }

        public void SetTickRate(double tickRate)
        {
            this.TickRate = tickRate;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
