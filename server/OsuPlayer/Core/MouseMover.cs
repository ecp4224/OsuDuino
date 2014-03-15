using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gma.System.Windows;
using System.Threading;

namespace OsuPlayer.Core
{
    public class MouseMover
    {
        private static Song playingSong;
        private static int startClicks = 3;
        public static bool Waiting
        {
            get;
            private set;
        }
        private static bool started = true;
        private static bool first = false;
        public static bool Playing
        {
            get;
            private set;
        }
        private static long startedTime;
        
        private static UserActivityHook mouseHook;
        
        public static void Init()
        {
            mouseHook = new UserActivityHook();
            mouseHook.OnMouseActivity += mouseHook_OnMouseActivity;
        }
        public static void Prepare(Song song) {
            playingSong = song;
            Waiting = true;
            startClicks = 3;
        }

        public static void Stop()
        {
            Playing = false;
        }

        static void mouseHook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Location.X + " : " + e.Location.Y);
            if (Waiting)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    //startClicks--;
                    //if (startClicks > 0)
                    //{
                    //    System.Windows.Forms.MessageBox.Show("" + startClicks);
                    //    return;
                   // }
                    Waiting = false;
                    first = true;
                    startedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    started = true;
                    Playing = true;
                    new Thread(new ThreadStart(ProcessSong)).Start();
                    //System.Windows.Forms.MessageBox.Show("Starting!");
                }
            }
        }

        static void ProcessSong()
        {
            Queue<Beat> temp = new Queue<Beat>();
            long start = playingSong.Beats.Peek().Time;
            long lastLeftClick = 0;
            bool leftDown = false;
            bool doingSlider = false;
            Beat previousBeat;
            while (playingSong.Beats.Count > 0 && Playing)
            {
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                milliseconds += start;
                long songTime = milliseconds - startedTime + 80;
                Beat beat = playingSong.Beats.Peek();

                if (leftDown && !doingSlider)
                {
                    long hold = (beat.Time - lastLeftClick) / 3L;
                    if (hold > 100 || hold < 10) hold = 80;
                    if (milliseconds - lastLeftClick >= hold)
                    {
                        Arduino.RequestLeftUp();
                        leftDown = false;
                    }
                }

                if (songTime > beat.Time)
                {
                    //if (!beat.IsSlider)
                    //{
                        Arduino.RequestLeftDown();
                        leftDown = true;
                        lastLeftClick = milliseconds;
                    //}
                    //else
                    //{
                    //    doingSlider = true;
                    //    Arduino.RequestLeftDown();
                    //}

                    previousBeat = playingSong.Beats.Dequeue();
                    temp.Enqueue(previousBeat);
                }
            }

            Arduino.RequestLeftUp();
            Arduino.RequestRightUp();

            while (playingSong.Beats.Count > 0)
            {
                temp.Enqueue(playingSong.Beats.Dequeue());
            }

            started = false;
            playingSong.Beats = temp;
            Playing = false;
        }
    }
}
