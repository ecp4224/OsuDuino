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
        public bool IsSlider
        {
            get
            {
                return SliderPoints != null;
            }
        }
        public Slider SliderPoints { get; private set; }
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

                    //if (line.Contains('|'))
                    //{
                    //    int repeat = 1;
                    //    int.TryParse(line.Split(',')[6], out repeat);
                    //    Slider slider = Slider.fromString(line.Split(',')[5], repeat);

                    //    beat.SliderPoints = slider;
                   // }
                    //else beat.SliderPoints = null;

                    beats.Enqueue(beat);
                }
            }

            return beats;
        }

        public class SliderData
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Slider
        {
            List<SliderData> points = new List<SliderData>();

            private Slider() { }
            public static Slider fromString(string str, int repeat)
            {
                Slider slider = new Slider();

                string[] data = str.Split('|');
                for (int i = 1; i < data.Length; i++)
                {
                    SliderData sData = new SliderData();
                    double x = 0, y = 0;

                    double.TryParse(data[i].Split(':')[0], out x);
                    double.TryParse(data[i].Split(':')[1], out y);

                    sData.X = x;
                    sData.Y = y;

                    slider.points.Add(sData);
                }

                if (repeat > 1)
                {
                    for (int z = 1; z < repeat; z++)
                    {
                        int start = z % 2 != 0 ? 0 : slider.points.Count - 1;
                        int end = z % 2 != 0 ? slider.points.Count : 0;
                        int add = z % 2 == 0 ? -1 : 1;
                        for (int i = start; (z % 2 == 0 && i >= end) || (z % 2 != 0 && i < end); i += add)
                        {
                            slider.points.Add(slider.points[i]);
                        }
                    }
                }

                return slider;
            }
        }
    }
}
