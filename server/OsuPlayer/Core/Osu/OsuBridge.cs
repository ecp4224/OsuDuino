using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WindowScrape.Types;

namespace OsuPlayer.Core.Osu
{
    public class OsuBridge
    {
        public delegate void FoundSongCallback(Song song);
        public static HwndObject OsuWindow { get; private set; }
        public static List<Song> Songs { get; private set; }
        public static void LoadAllSongs(FoundSongCallback callback = null)
        {
            string osuDir = "C:\\Program Files\\osu!\\Songs";
            if (!Directory.Exists(osuDir))
            {
                osuDir = "C:\\Program Files (x86)\\osu!\\Songs";
                if (!Directory.Exists(osuDir)) return;
            }

            string[] folders = Directory.GetDirectories(osuDir);
            for (int i = 0; i < folders.Length; i++)
            {
                string folder = folders[i];

                string[] files = Directory.GetFiles(folder, "*.osu");
                foreach (string file in files)
                {
                    Song song = new Song()
                    {
                        Name = Path.GetFileName(file).Split('.')[0],
                        Path = file
                    };
                    double tickRate = 1;
                    song.Beats = Beat.FromSong(song, out tickRate);
                    song.SetTickRate(tickRate);
                    song.LoadTimers();
                    if (callback != null)
                        callback(song);
                    
                }
            }
        }

        public static void SearchForOsuWindow()
        {
            OsuWindow = HwndObject.GetWindowByTitle("osu!");
        }

        
    }
}
