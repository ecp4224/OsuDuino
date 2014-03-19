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
using OsuPlayer.Core.Osu;

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
            comboBox1.Sorted = true;
            Setup();
            MouseMover.Init();
            timer1.Start();
        }

        ~MainMenu()
        {
            timer1.Stop();
            timer1.Dispose();
        }

        private ProgressDialog search;
        void Setup()
        {
            setStatusText("Running setup method..");
            search = new ProgressDialog()
            {
                WindowTitle = "Osu! Player..",
                Description = "Hooking into Osu! window..",
                ShowTimeRemaining = true,
                ShowCancelButton = false,
                ProgressBarStyle = Ookii.Dialogs.ProgressBarStyle.MarqueeProgressBar
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

            while (true)
            {
                try
                {
                    Thread.Sleep(3000);
                    OsuBridge.SearchForOsuWindow();
                    if (OsuBridge.OsuWindow.Size.Width == 0 && OsuBridge.OsuWindow.Size.Height == 0)
                    {
                        DialogResult result = MessageBox.Show("There was a problem with hooking into the osu! window.\nIs it open?", "OsuDuino", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                        if (result == System.Windows.Forms.DialogResult.Abort) return;
                        else if (result == System.Windows.Forms.DialogResult.Retry) continue;
                        else break;
                    }
                    search.ReportProgress(0, "Found 0 songs", "Please wait while I search for osu! songs..");
                    break;
                }
                catch
                {
                    DialogResult result = MessageBox.Show("There was a problem with hooking into the osu! window.\nIs it open?", "OsuDuino", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    if (result == System.Windows.Forms.DialogResult.Abort) return;
                    else if (result == System.Windows.Forms.DialogResult.Retry) continue;
                    else break;
                }
            }

            int count = 0;
            OsuBridge.LoadAllSongs(new OsuBridge.FoundSongCallback(delegate(Song song)
            {
                AddItem(song);
                count++;
                search.ReportProgress(0, "Found " + count + " song" + (count != 1 ? "s" : ""), null);
            }));

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
            if (!MouseMover.Playing)
            {
                if (comboBox1.SelectedIndex == -1) return;
                MouseMover.Prepare((Song)comboBox1.SelectedItem);
            }
            else
            {
                MouseMover.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MouseMover.Playing)
                button1.Text = "Stop";
            else if (MouseMover.Waiting)
                button1.Text = "Waiting..";
            else
                button1.Text = "Start";

            curPos.Text = "Mouse X: " + MouseMover.CurrentX + " Mouse Y: " + MouseMover.CurrentY;
            label2.Text = "Target X: " + MouseMover.TargetX + " Target Y: " + MouseMover.TargetY;
        }
    }
}
