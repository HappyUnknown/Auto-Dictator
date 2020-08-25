﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Chord_Dictator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Chord> chords = new List<Chord>();
        public DispatcherTimer timer = new DispatcherTimer();
        bool mute = false;
        string dfp = "App Files/Dictionaries/chordList.txt";
        string log = "App Files/Initial Files/logs.txt";
        string defaultImage = "App Files/Initial Files/defimage.png";
        string defaultAudio = "App Files/Initial Files/defaudio.wav";
        List<int> alreadyShown = new List<int>();
        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            File.WriteAllText(log, "");
            WriteToLog("Program launched.");
        }
        struct Chord
        {
            public string name;
            public string imagePath;
            public string soundPath;
            public Chord(string n, string i, string s)
            {
                name = n;
                imagePath = i;
                soundPath = s;
            }
        }
        void InitTimer()
        {
            timer.Tick += Start;
            timer.Interval = TimeSpan.FromSeconds(10);
        }
        int StrToInt(string str)
        {
            int num;
            if (!int.TryParse(str, out num))
            {
                WriteToLog("Failed to parse string \"" + str + "\" to int.");
                return 5;
            }
            return num;
        }
        void CreateIfNoDir()
        {
            if (!Directory.Exists("App Files"))
            {
                Directory.CreateDirectory("App Files");
                if (!Directory.Exists("App Files"))
                {
                    Directory.CreateDirectory("App Files/Directory");
                    if (!File.Exists(dfp))
                    {
                        File.Create(dfp);
                    }
                }
            }
        }
        void WriteToLog(string message, string exmsg = "", string tip = "")
        {
            if (!File.Exists(log)) File.Create(log);
            File.AppendAllText(log, "[" + DateTime.Now + "] -> " + message);
            if (exmsg.Length > 0) File.AppendAllText(log, " (" + exmsg + ") ");
            if (tip.Length > 0) File.AppendAllText(log, "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText(log, Environment.NewLine);
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(StrToInt(tbDelay.Text));
                CreateIfNoDir();
                string[] rawChords = File.ReadAllLines(dfp);
                string[] currChord;
                chords.Clear();
                for (int i = 0; i < rawChords.Length; i++)
                {
                    currChord = rawChords[i].Split();
                    chords.Add(new Chord(currChord[0].Replace('>', ' '), currChord[1].Replace('>', ' '), currChord[2].Replace('>', ' ')));
                }
                WriteToLog("New session started.");
                try
                {
                    Dispatcher.Invoke(() => timer.Start());
                    if (File.ReadAllText(dfp).TrimEnd(' ').Length == 0)
                    {
                        MessageBox.Show("Dictionary is empty.");
                        WriteToLog("Dictionary " + dfp + " is empty.");
                        timer.Stop();
                        return;
                    }
                    if (alreadyShown.Count > 0) alreadyShown.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Start failed.");
                    WriteToLog("Failed to start timer.", ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Start failed.");
                WriteToLog("Failed to reach dictionary file (" + dfp + ").", ex.Message);
                if (MessageBox.Show("Do you want to create dictionary file?", "No such file", MessageBoxButton.YesNo) == MessageBoxResult.Yes) File.Create(dfp);
                else return;
            }
        }
        bool Unique(int num)
        {
            foreach (var el in alreadyShown)
            {
                if (num == el) return false;
            }
            return true;
        }
        void Start(object sender, EventArgs e)
        {
            int randomIndex = new Random().Next(0, chords.Count);
            if (File.ReadAllLines(dfp).Length <= alreadyShown.Count && alreadyShown.Count != 0)
            {
                Dispatcher.Invoke(() => timer.Stop());
                MessageBox.Show("Dictation finished successfuly.");
                WriteToLog("Dictation finished successfuly.");
                timer.Stop();
                return;
            }
            while (!Unique(randomIndex))
            {
                randomIndex = new Random().Next(0, chords.Count);
            }
            alreadyShown.Add(randomIndex);
            try
            {
                imgChord.Source = new BitmapImage(new Uri(chords[randomIndex].imagePath.Replace('>', ' ')));
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to load image.", ex.Message);
            }
            tbChordName.Text = chords[randomIndex].name;
            try
            {
                if (!mute)
                {
                    SoundPlayer sp = new SoundPlayer(chords[randomIndex].soundPath);
                    sp.Play();
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to load sound.", ex.Message);
            }
        }
        private void btnGoToAdd_Click(object sender, RoutedEventArgs e)
        {
            AddWindow window = new AddWindow(dfp, log, defaultImage, defaultAudio);
            WriteToLog("Opened add/remove window.");
            window.ShowDialog();
        }

        private void btnChangeInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();
                if (ofd.FileName.Length > 3)
                {
                    if (ofd.FileName.Substring(ofd.FileName.Length - 4, 4) == ".txt")
                    {
                        dfp = ofd.FileName;
                        MessageBox.Show("Initial file changed to " + dfp);
                        WriteToLog("Initial file changed to " + dfp);
                        btnGoToAdd.ToolTip = "All added items will be saved to " + dfp;
                    }
                    else
                    {
                        WriteToLog("Failed to change initial dictionary. File type does not match.");
                        MessageBox.Show("File type does not match.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                WriteToLog("Failed to change initial dictionary.", ex.Message);
            }
        }
    }
}