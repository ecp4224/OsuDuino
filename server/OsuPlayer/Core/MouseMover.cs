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
        private static bool waiting = false;
        private static bool started = true;
        private static bool first = false;
        private static long startedTime;
        private static UserActivityHook mouseHook;
        public static void Init()
        {
            mouseHook = new UserActivityHook();
            mouseHook.OnMouseActivity += mouseHook_OnMouseActivity;
        }
        public static void Prepare(Song song) {
            playingSong = song;
            waiting = true;
            startClicks = 3;
        }

        static void mouseHook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (waiting)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    //startClicks--;
                    //if (startClicks > 0)
                    //{
                    //    System.Windows.Forms.MessageBox.Show("" + startClicks);
                    //    return;
                   // }
                    waiting = false;
                    first = true;
                    startedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    started = true;
                    new Thread(new ThreadStart(ProcessSong)).Start();
                    //System.Windows.Forms.MessageBox.Show("Starting!");
                }
            }
        }

        static void ProcessSong()
        {
            long start = playingSong.Beats.Peek().Time;
            while (playingSong.Beats.Count > 0)
            {
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                milliseconds += start;
                long songTime = milliseconds - startedTime;
                Beat beat = playingSong.Beats.Peek();

                if (songTime > beat.Time)
                {
                    Arduino.RequestLeftClick();
                    playingSong.Beats.Dequeue();
                }
            }
            started = false;
        }
    }
}
