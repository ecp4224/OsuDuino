using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OsuPlayer.Core.Osu
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

        public static Queue<Beat> FromSong(Song song, out double tickRate)
        {
            tickRate = 1;
            Queue<Beat> beats = new Queue<Beat>();
            string[] lines = File.ReadAllLines(song.Path);
            bool start = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("SliderTickRate") && line.Split(':').Length > 1)
                {
                    double.TryParse(line.Split(':')[1], out tickRate);
                }
                else if (line == "[HitObjects]")
                {
                    start = true;
                    continue;
                }
                else if (start)
                {
                    Beat beat = new Beat();
                    long time = 0;
                    int x = 0, y = 0;
                    long.TryParse(line.Split(',')[2], out time);
                    int.TryParse(line.Split(',')[0], out x);
                    int.TryParse(line.Split(',')[1], out y);
                    beat.Time = time;
                    beat.X = x;
                    beat.Y = y;

                    if (line.Contains('|') && line.Split(',').Length > 5)
                    {
                        try
                        {
                            int repeat = 1;
                            if (line.Split(',').Length > 6) int.TryParse(line.Split(',')[6], out repeat);
                            
                            Slider slider = Slider.fromString(line.Split(',')[5], repeat);
                            
                            int temp = 100;
                            if (line.Split(',').Length > 7) int.TryParse(line.Split(',')[7], out temp);
                            slider.LengthToEnd = temp;

                            beat.SliderPoints = slider;
                        }
                        catch (Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show(e.ToString());
                        }
                    }
                    else beat.SliderPoints = null;

                    beats.Enqueue(beat);
                }
            }

            return beats;
        }

        public class SliderData
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Slider
        {
            public bool PressAndHold { get; private set; }
            public int LengthToEnd;
            public List<SliderData> points = new List<SliderData>();

            private Slider() { }
            public static Slider fromString(string str, int repeat)
            {
                Slider slider = new Slider();

                string[] data = str.Split('|');
                for (int i = 1; i < data.Length; i++)
                {
                    SliderData sData = new SliderData();
                    int x = 0, y = 0;

                    int.TryParse(data[i].Split(':')[0], out x);
                    int.TryParse(data[i].Split(':')[1], out y);

                    sData.X = x;
                    sData.Y = y;
                    try
                    {
                        slider.points.Add(sData);
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("" + slider.points.Count + " : " + repeat);
                    }
                }
                if (repeat >= 25)
                {
                    slider.PressAndHold = true;
                }
                else if (repeat > 1)
                {
                    for (int z = 1; z < repeat; z++)
                    {
                        int start = z % 2 != 0 ? 0 : slider.points.Count - 1;
                        int end = z % 2 != 0 ? slider.points.Count : 0;
                        int add = z % 2 == 0 ? -1 : 1;
                        for (int i = start; (z % 2 == 0 && i >= end) || (z % 2 != 0 && i < end); i += add)
                        {
                            try
                            {
                                slider.points.Add(slider.points[i]);
                            }
                            catch
                            {
                                System.Diagnostics.Debug.WriteLine("" + slider.points.Count + " : " + repeat);
                            }
                        }
                    }
                }

                return slider;
            }
        }
    }
}
