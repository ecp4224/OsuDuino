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
        private static Song playingSong; //The current song playing
        public const bool ISITWORKING = false; //A fail safe incase the robot won't move during presentation
        public static bool Waiting //Whether the server is waiting for a right click to start the song
        {
            get;
            private set;
        }
        public static bool Playing //Whether the song is playing or not
        {
            get;
            private set;
        }
        private static long startedTime; //The time the right click occured in milliseconds
        public static int TargetX //Where the Arduino needs to go on the X
        {
            get;
            private set;
        }
        public static int TargetY //Where the Arduino needs to go on the Y
        {
            get;
            private set;
        }
        public static int CurrentX //The current X position of the mouse
        {
            get;
            private set;
        }
        public static int CurrentY //The current Y position of the mouse
        {
            get;
            private set;
        }
        
        private static UserActivityHook mouseHook; //An instance of the UserActivityHook class to get global mouse events
        
        public static void Init()
        {
            mouseHook = new UserActivityHook(); //Create a new UserActivityHook object to get global mouse events
            mouseHook.OnMouseActivity += mouseHook_OnMouseActivity; //Register for the mouse activity event and use the callback method "mouseHook_OnMouseActivity"
        }
        public static void Prepare(Song song)
        {
            playingSong = song; //Set the current song playing to the one provided
            Waiting = true; //Wait for the right click
        }

        public static void Stop()
        {
            Playing = false; //Set playing to False to cut the main loop
        }

        static int TranslateX(int x)
        {
            /*
             *    MouseX * osu!WindowWidth
             * x = ----------------------- to translate to the resolution of the window
             *            640
             * x = x + osu!WindowX + 75 to account for the screen location and the margin 
             * 
             */
            return (((x * OsuBridge.OsuWindow.Size.Width) / 640) + OsuBridge.OsuWindow.Location.X) + 75; //Translate the osu! beat x position to the screen's position
        }

        static int TranslateY(int y)
        {
            /*
             *    MouseY * osu!WindowHeight
             * y = ----------------------- to translate to the resolution of the window
             *            480
             * y = y + osu!WindowY + 75 to account for the screen location and the margin 
             * 
             */
            return (((y * OsuBridge.OsuWindow.Size.Height) / 480) + OsuBridge.OsuWindow.Location.Y) + 75; //Translate the osu! beat y position to the screen's position
        }

        static void mouseHook_OnMouseActivity(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CurrentX = e.Location.X; //Set the CurrentX to the current mouse X position
            CurrentY = e.Location.Y; //Set the CurrentY to the current mouse Y position
            if (Waiting) //If were waiting for the right click
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right) //And the user did right click
                {
                    Waiting = false; //Were no longer waiting
                    startedTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; //Get the current time in Milliseconds
                    Playing = true; //Now were playing
                    new Thread(new ThreadStart(ProcessSong)).Start(); //Execute the method ProcessSong in a seperate processing thread
                }
            }
        }

        static void ProcessSong()
        {
            Queue<Beat> temp = new Queue<Beat>(); //Keep a cache of the notes that have been played, so we can add them back when were done
            long start = playingSong.Beats.Peek().Time; //Set the current song time to the first beat, because if the user clicked the first beat, we are at the position in the song
            long lastLeftClick = 0; //Last left click in milliseconds
            bool leftDown = false; //The left mouse button is not down
            bool doingSlider = false; //Were not doing a slider yet
            int pointIndex = 0; //Which point on the slider were on
            long lastSlide = 0; //The last slide change in milliseconds
            long duration = 80; //How long to hold down the mouse button
            Beat previousBeat = null; //We haven't had a last beat yet
            while (playingSong.Beats.Count > 0 && Playing) //While we have notes to play and were still playing (Stop hasn't been called)
            {
                //CHECK CLICK
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; //The current time in milliseconds
                milliseconds += start; //Push it by the start variable
                long songTime = milliseconds - startedTime + 80; //The song time will be the current time, minus the time when we started, plus 80 (for latency)
                playingSong.UpdateTimer(songTime); //Songs can have different speeds at different times, check to see if the timing has changed
                Beat beat = playingSong.Beats.Peek(); //Get the next beat without removing it
                Timing timer = playingSong.CurrentTimer; //Get the current timing for the song

                //SET TARGETS
                if (doingSlider) //If were doing a slider
                {
                    if (previousBeat == null || !previousBeat.IsSlider) //If the last beat wasn't a slider or we don't have a previous beat
                    {
                        doingSlider = false; //Then we're clearly not doing a slider
                        leftDown = false; //And the left mouse is not down
                        Arduino.RequestLeftUp(); //Tell the Arduino to lift up
                    }
                    else //Otherwise
                    {
                        long diff = songTime - lastSlide; //How long has it been since we started this slide or since the last point
                        if (timer.CalculateBeats(diff) >= 1) //If its been a full beat
                        {
                            pointIndex++; //Increase the point on the slider
                            if (pointIndex < previousBeat.SliderPoints.points.Count) //If we have another point after this
                            {
                                lastSlide = songTime; //Set the last slide to the current song time
                                TargetX = TranslateX(previousBeat.SliderPoints.points[pointIndex].X); //Set the target X to the new point
                                TargetY = TranslateY(previousBeat.SliderPoints.points[pointIndex].Y); //Set the target Y to the new point
                            }
                        }
                    }
                }
                else //Otherwise
                {
                    TargetX = TranslateX(beat.X); //Set the target X to the next beat
                    TargetY = TranslateY(beat.Y); //Set the target Y to the next beat
                }

                //CLICK TIMING
                if (leftDown) //If the left mouse button is down
                {
                    if (doingSlider) //If were doing a slider
                    {
                        long time = (milliseconds - lastLeftClick); //How long has it been since we started this click
                        if (time >= duration) //If its greater than or equal to the time we to hold this note down
                        {
                            Arduino.RequestLeftUp(); //Tell the arduino to lift up
                            leftDown = false; //We're no longer pressing down
                            doingSlider = false; //We're not doing a slider
                            previousBeat = null; //We no longer need a previous beat
                        }
                    }
                    else //Otherwise
                    {
                        long hold = (beat.Time - lastLeftClick) / 3L; //Calculate the time between the last click and the next beat. We want to hold down the left mouse button for a 3rd of that time
                        if (hold > 100 || hold < 10) hold = 80; //However, if we go over 100ms or go under 10ms, then default to 80ms
                        if (milliseconds - lastLeftClick >= hold || beat.Time - songTime < 30) //If we've been holding down the button for the calculated time OR the next beat is 30ms away
                        {
                            Arduino.RequestLeftUp(); //Tell the arduino to lift up.
                            leftDown = false; //We're no longer pressing down
                        }
                    }
                }

                if (songTime > beat.Time) //If the current song time is past a beat's time
                {
                    //WE NEED TO CLICK, THERES A BEAT!
                    if (!beat.IsSlider)  //Is this beat NOT a slider?
                    {
                        Arduino.RequestLeftDown(); //Tell the arduino to press down
                        leftDown = true; //We're pressing the left mouse button
                        lastLeftClick = milliseconds; //The last time we clicked the left mouse button is NOW
                        previousBeat = playingSong.Beats.Dequeue(); //Set the previous beat to this beat (and remove it from the list Beats)
                    }
                    else //Otherwise
                    {
                        doingSlider = true; //We're doing a slider
                        leftDown = true; //And the left mouse button is down
                        pointIndex = 0; //We are at the first point of the slider
                        lastLeftClick = milliseconds; //The last time we clicked the left mouse button is NOW
                        previousBeat = playingSong.Beats.Dequeue(); //Set the previous beat to this beat (and remove it from the list Beats)
                        /*
                         * MillisecondsPerBeat = the amount of milliseconds that has to pass for 1 beat
                         * beat.SliderPoints.LengthToEnd = How long the slider is in pixels
                         * And osu! moves a slider 100 pixels PER beat
                         * 
                         * So
                         * We multiply MillisecondsPerBeat to the length of the slider over 100
                        */
                        duration = (long)(timer.MillisecondsPerBeat * (beat.SliderPoints.LengthToEnd / 100.0));
                        duration *= beat.SliderPoints.Repeats; //And multiply it by how many times the slider repeats
                        if (playingSong.Beats.Count > 0) //If we have another beat after this
                        {
                            /*
                             * We want time to have time to lift up and press down for the next note
                             * 
                             * So, we subtract the current duration by half the distance of this note to the next note
                             * 
                             * Because it just works
                            */
                            duration -= (long)((timer.MillisecondsPerBeat / 2.0) - (playingSong.Beats.Peek().Time - (songTime + duration)));
                        }
                        else //Otherwise
                        {
                            //For latency, subtract 80
                            duration -= 80;
                        }
                        System.Diagnostics.Debug.WriteLine(duration); //For debugging..
                        lastSlide = songTime; //The last slide time was NOW
                        Arduino.RequestLeftDown(); //Tell the arduino to press down
                    }

                    temp.Enqueue(previousBeat); //Add the previous beat to the temp queue
                }

                if (ISITWORKING) //Are the motors finally working?
                {
                    //CHECK MOVEMENT
                    if (Math.Abs(CurrentX - TargetX) < 5) //If were 5 pixels away from our X target
                    {
                        Arduino.StopX(); //Stop the X motor
                    }
                    else //Otherwise
                    {
                        if (CurrentX > TargetX) //If we are past our target
                            Arduino.MoveLeft(); //Move left
                        else //Otherwise
                            Arduino.MoveRight(); //Move right
                    }

                    if (Math.Abs(CurrentY - TargetY) < 5) //If were 5 pixels away from our Y target
                    {
                        Arduino.StopY(); //Stop the Y motor
                    }
                    else //Otherwise
                    {
                        if (CurrentY > TargetY) //If we are past our target
                            Arduino.MoveUp(); //Move up
                        else //Otherwise
                            Arduino.MoveDown(); //Move down
                    }
                }
            }
            //The song has ended because the loop broke
            Arduino.RequestLeftUp(); //Stop pressing the mouse button
            Arduino.StopX(); //Stop moving x
            Arduino.StopY(); //Stop moving y

            while (playingSong.Beats.Count > 0) //If we still have notes (Stop was executed)
            {
                temp.Enqueue(playingSong.Beats.Dequeue()); //Put the remaining notes into the temp queue
            }
            playingSong.Beats = temp; //Set this song's Beat list to the temp queue
            Playing = false; //We are no longer playing
        }
    }
}
