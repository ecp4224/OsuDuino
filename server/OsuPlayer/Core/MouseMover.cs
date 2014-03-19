using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gma.System.Windows;
using System.Threading;
using OsuPlayer.Core.Osu;

namespace OsuPlayer.Core
{
    public class MouseMover
    {
        private static Song playingSong;
        public const bool ISITWORKING = false;
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
        public static int TargetX
        {
            get;
            private set;
        }
        public static int TargetY
        {
            get;
            private set;
        }
        public static int CurrentX
        {
            get;
            private set;
        }
        public static int CurrentY
        {
            get;
            private set;
        }
        
        private static UserActivityHook mouseHook;
        
        public static void Init()
        {
            mouseHook = new UserActivityHook();
            mouseHook.OnMouseActivity += mouseHook_OnMouseActivity;
        }
        public static void Prepare(Song song)
        {
            playingSong = song;
            Waiting = true;
            startClicks = 3;
        }

        public static void Stop()
        {
            Playing = false;
        }

        static int TranslateX(int x)
        {
            return (((x * OsuBridge.OsuWindow.Size.Width) / 640) + OsuBridge.OsuWindow.Location.X) + 75;
        }

        static int TranslateY(int y)
        {
            return (((y * OsuBridge.OsuWindow.Size.Height) / 480) + OsuBridge.OsuWindow.Location.Y) + 75;
        }

        static void mouseHook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CurrentX = e.Location.X;
            CurrentY = e.Location.Y;
            if (Waiting)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    Waiting = false;
                    first = true;
                    startedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    started = true;
                    Playing = true;
                    new Thread(new ThreadStart(ProcessSong)).Start();
                }
            }
        }

        static void MoveMouse()
        {
            while (Playing)
            {
                
                Thread.Sleep(1);
            }
        }

        static void ProcessSong()
        {
            Queue<Beat> temp = new Queue<Beat>();
            long start = playingSong.Beats.Peek().Time;
            long lastLeftClick = 0;
            bool leftDown = false;
            bool doingSlider = false;
            int pointIndex = 0;
            long lastSlide = 0;
            long duration = 80;
            Beat previousBeat = null;
            while (playingSong.Beats.Count > 0 && Playing)
            {
                //CHECK CLICK
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                milliseconds += start;
                long songTime = milliseconds - startedTime + 80;
                playingSong.UpdateTimer(songTime);
                Beat beat = playingSong.Beats.Peek();
                Timing timer = playingSong.CurrentTimer;


                if (doingSlider)
                {
                    if (previousBeat == null || !previousBeat.IsSlider)
                    {
                        doingSlider = false;
                        leftDown = false;
                        Arduino.RequestLeftUp();
                    }
                    else
                    {
                        long diff = songTime - lastSlide;
                        if (timer.CalculateBeats(diff) >= 1)
                        {
                            pointIndex++;
                            if (pointIndex < previousBeat.SliderPoints.points.Count)
                            {
                                lastSlide = songTime;
                                TargetX = TranslateX(previousBeat.SliderPoints.points[pointIndex].X);
                                TargetY = TranslateY(previousBeat.SliderPoints.points[pointIndex].Y);
                            }
                        }
                    }
                }
                else
                {
                    TargetX = TranslateX(beat.X);
                    TargetY = TranslateY(beat.Y);
                }

                if (leftDown)
                {
                    if (doingSlider)
                    {
                        long time = (milliseconds - lastLeftClick);
                        if (time >= duration)
                        {
                            Arduino.RequestLeftUp();
                            leftDown = false;
                            doingSlider = false;
                            previousBeat = null;
                        }
                    }
                    else
                    {
                        long hold = (beat.Time - lastLeftClick) / 3L;
                        if (hold > 100 || hold < 10) hold = 80;
                        if (milliseconds - lastLeftClick >= hold || beat.Time - songTime < 30)
                        {
                            Arduino.RequestLeftUp();
                            leftDown = false;
                        }
                    }
                }

                if (songTime > beat.Time)
                {
                    if (!beat.IsSlider)
                    {
                        Arduino.RequestLeftDown();
                        leftDown = true;
                        lastLeftClick = milliseconds;
                        previousBeat = playingSong.Beats.Dequeue();
                    }
                    else
                    {
                        doingSlider = true;
                        leftDown = true;
                        pointIndex = 0;
                        lastLeftClick = milliseconds;
                        previousBeat = playingSong.Beats.Dequeue();
                        duration = (long)(timer.MillisecondsPerBeat * (beat.SliderPoints.LengthToEnd / 100.0));
                        duration *= beat.SliderPoints.Repeats;
                        if (playingSong.Beats.Count > 0)
                        {
                            duration -= (long)((timer.MillisecondsPerBeat / 2.0) - (playingSong.Beats.Peek().Time - (songTime + duration)));
                        }
                        else
                        {
                            duration -= 80;
                        }
                        System.Diagnostics.Debug.WriteLine(duration);
                        lastSlide = songTime;
                        Arduino.RequestLeftDown();
                    }

                    temp.Enqueue(previousBeat);
                }

                if (ISITWORKING)
                {
                    //CHECK MOVEMENT
                    if (Math.Abs(CurrentX - TargetX) < 5)
                    {
                        Arduino.StopX();
                    }
                    else
                    {
                        if (CurrentX > TargetX)
                            Arduino.MoveLeft();
                        else
                            Arduino.MoveRight();
                    }

                    if (Math.Abs(CurrentY - TargetY) < 5)
                    {
                        Arduino.StopY();
                    }
                    else
                    {
                        if (CurrentY > TargetY)
                            Arduino.MoveUp();
                        else
                            Arduino.MoveDown();
                    }
                }
            }

            Arduino.RequestLeftUp();
            Arduino.StopX();
            Arduino.StopY();

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
