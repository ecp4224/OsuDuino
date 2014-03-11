using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using OsuPlayer.Core;
using Ookii.Dialogs;

namespace OsuPlayer
{
    public partial class MainMenu : Form
    {
        public delegate void StringMethod(string text);
        public delegate void SongMethod(Song song);
        
        public MainMenu()
        {
            InitializeComponent();
            button1.Enabled = false;
            Setup();
            MouseMover.Init();
        }

        private ProgressDialog search;
        void Setup()
        {
            setStatusText("Running setup method..");
            search = new ProgressDialog()
            {
                WindowTitle = "Osu! Player..",
                Description = "Please wait while I search for osu! songs..",
                Text = "Found 0 songs",
                ShowTimeRemaining = true,
                ShowCancelButton = false
            };
            search.DoWork += search_DoWork;

            search.RunWorkerCompleted += delegate
            {
                button1.Enabled = true;
                Thread thread = new Thread(new ThreadStart(ConnectArduino));
                thread.Start();

                search.Dispose();
            };

            search.Show(this);
        }

        void search_DoWork(object sender, DoWorkEventArgs e)
        {
            setStatusText("Searching for osu! songs..");

            string osuDir = "C:\\Program Files\\osu!\\Songs";
            if (!Directory.Exists(osuDir))
            {
                osuDir = "C:\\Program Files (x86)\\osu!\\Songs";
                if (!Directory.Exists(osuDir)) return;
            }

            string[] folders = Directory.GetDirectories(osuDir);
            int percent = 0;
            int count = 0;
            for (int i = 0; i < folders.Length; i++)
            {
                string folder = folders[i];
                percent = (int)((double)((double)i / (double)folders.Length) * 100.0);
                search.ReportProgress(percent, null, null);


                string[] files = Directory.GetFiles(folder, "*.osu");
                foreach (string file in files)
                {
                    Song song = new Song()
                    {
                        Name = Path.GetFileName(file).Split('.')[0],
                        Path = file
                    };
                    song.Beats = Beat.FromSong(song);
                    AddItem(song);
                    count++;
                    search.ReportProgress(percent, "Found " + count + " song" + (count != 1 ? "s" : ""), null);
                }
            }

            setStatusText("Done!");
        }

        void ConnectArduino()
        {
            while (true)
            {
                setStatusText("Waiting for user input...");
                string port = null;
                while (true)
                {
                    InputDialog ask = new InputDialog();
                    ask.Content = "The Arduino port must be specified. Please enter the port the Arduino device is connected to (Example: COM4)";
                    ask.MainInstruction = "Please enter the Arduino port";
                    ask.WindowTitle = "Arduino port";
                    if (ask.ShowDialog() == DialogResult.OK)
                    {
                        port = ask.Input;
                    }
                    else return;
                    if (!String.IsNullOrEmpty(port)) break;
                }
                setStatusText("Connecting to Arduino on port " + port + "...");
                try
                {
                    Arduino.Init(port);
                }
                catch
                {
                    continue;
                }
                setStatusText("Connected!");
                break;
            }

        }

        void setStatusText(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new StringMethod(setStatusText), new object[] { text });
                return;
            }

            toolStripStatusLabel1.Text = text;
        }

        void AddItem(Song song)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SongMethod(AddItem), new object[] { song });
                return;
            }
            this.comboBox1.Items.Add(song);
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                button1.Enabled = false;
            else
                button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1) return;
            MouseMover.Prepare((Song)comboBox1.SelectedItem);
        }
    }


    public class Song
    {
        public string Path;
        public string Name;
        public Queue<Beat> Beats;

        public override string ToString()
        {
            return Name;
        }
    }
}
