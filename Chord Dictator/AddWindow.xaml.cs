using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chord_Dictator
{
    /// <summary>
    /// Логика взаимодействия для AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        string log;
        string dfp;
        string defaultImage;
        string defaultAudio;
        public AddWindow()
        {
            InitializeComponent();
        }
        public AddWindow(string path, string logs, string defim, string defau)
        {
            InitializeComponent();
            dfp = path;
            log = logs;
            defaultImage = defim;
            defaultAudio = defau;
        }
        void WriteToLog(string message, string exmsg = "", string tip = "")
        {
            if (!File.Exists(log)) File.Create(log).Close();
            File.AppendAllText(log, "[" + DateTime.Now + "] -> " + message);
            if (exmsg.Length > 0) File.AppendAllText(log, " (" + exmsg + ") ");
            if (tip.Length > 0) File.AppendAllText(log, "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText(log, Environment.NewLine);
        }
        public void AddChord(string n, string i, string s)
        {
            File.WriteAllText(dfp, n + ">" + i + ">" + s + Environment.NewLine);
        }
        void CreateIfNoDl()
        {
            if (!Directory.Exists("App Files"))
            {
                Directory.CreateDirectory("App Files");
            }
            if (!Directory.Exists("App Files"))
            {
                Directory.CreateDirectory("App Files/Directory");
            }
            if (!File.Exists(dfp))
            {
                File.Create(dfp).Close();
                WriteToLog("Initial dictionary file created.");
            }
        }
        private void btnConnImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "PNG Picture|*.png|JPEG Picture|*.jpeg|JPG Picture|*.jpg|All|*.*";
                if (ofd.ShowDialog() == true)
                {
                    imgChord.Source = new BitmapImage(new Uri(ofd.FileName));
                    tbChordImgPath.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                WriteToLog("Failed to connect image.", ex.Message);
            }
        }
        string GetFileName(string path)
        {
            for (int i = path.Length - 1; i >= 0; i++)
            {
                if (path[i] == '\\')
                {
                    return path.Substring(i, path.Length - i);
                }
            }
            return "NO_NAME";
        }
        string GetFileHome(string path)
        {
            for (int i = path.Length - 1; i >= 0; i++)
            {
                if (path[i] == '\\')
                {
                    return path.Substring(0, i);
                }
            }
            return "NO_PATH";
        }
        private void btnConnSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "WAV Sound|*.wav|All|*.*";
                if (ofd.ShowDialog() == true)
                {
                    tbSoundPath.Text = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                File.AppendAllText(log, "Failed to connect sound: " + DateTime.Now + "-> " + ex.Message + Environment.NewLine);
            }
        }
        private void btnAddChord_Click(object sender, RoutedEventArgs e)
        {
            AddChord();
        }
        void AddChord()
        {
            try
            {
                if (tbName.Text == "")
                {
                    MessageBox.Show("No name signed. Rename please.");
                    return;
                }
                if (tbSoundPath.Text == "")
                {
                    tbSoundPath.Text = defaultAudio;
                    MessageBox.Show("No sound signed.");
                }
                if (imgChord.Source == null || tbChordImgPath.Text == "")
                {
                    tbChordImgPath.Text = defaultImage;
                    MessageBox.Show("No image signed.");
                }

                if (!tbName.Text.Contains('>') && !tbChordImgPath.Text.Contains('>') && !tbSoundPath.Text.Contains('>'))
                {
                    File.AppendAllText(dfp, tbName.Text + ">" + tbChordImgPath.Text + ">" + tbSoundPath.Text + Environment.NewLine);
                    MessageBox.Show("Added \"" + tbName.Text + "\" element. Path: " + dfp);
                    imgChord.Source = null;
                    tbName.Text = string.Empty;
                    tbSoundPath.Text = string.Empty;
                    tbChordImgPath.Text = string.Empty;
                    WriteToLog("Added new element \"" + tbName.Text + "\".");
                }
                else
                {
                    MessageBox.Show("\">\" program symbol used. Try removing it.");
                    WriteToLog("Program symbol used in dictionary addition.");
                }
                Close();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add chord.", ex.Message, "Try recreating initial dictionary or trying again");
                CreateIfNoDl();
            }
        }
        private void btnRmLast_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (MessageBox.Show("Do you want to remove last element?", "Removing last", MessageBoxButton.YesNo) == MessageBoxResult.Yes && File.ReadAllLines(dfp).Length > 0)
                {
                    List<string> content = File.ReadAllLines(dfp).ToList();
                    MessageBox.Show("Last chord removed. - " + content[content.Count - 1].Split('>')[0]);
                    content.RemoveAt(content.Count - 1);
                    File.WriteAllLines(dfp, content.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't remove");
                CreateIfNoDl();
                WriteToLog("Failed to remove last from dictionary.", ex.Message, "Your dictionary file was recreated, because program did not reach it. Try again.");
            }
        }
        #region TEST
        void AddChordPortable()
        {
            try
            {

                if (tbName.Text == "")
                {
                    MessageBox.Show("No name signed.");
                    return;
                }
                if (tbSoundPath.Text == "")
                {
                    MessageBox.Show("No sound signed.");
                    return;
                }
                if (imgChord.Source == null || tbChordImgPath.Text == "")
                {
                    MessageBox.Show("No image signed.");
                    return;
                }
                else
                {
                    if (!tbName.Text.Contains('>'))
                    {
                        File.AppendAllText(dfp, tbName.Text + ">" + FileToBase64(tbChordImgPath.Text) + ">" + FileToBase64(tbSoundPath.Text) + Environment.NewLine);
                        MessageBox.Show("Added \"" + tbName.Text + "\" chord.");
                        imgChord.Source = null;
                        tbName.Text = string.Empty;
                        tbSoundPath.Text = string.Empty;
                        tbChordImgPath.Text = string.Empty;
                    }
                    else
                    {
                        WriteToLog("Failed to add chord due to \">\" was used.");
                        MessageBox.Show("\">\" program symbol used. Try removing it.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add chord.", ex.Message, "Try recreating initial dictionary or trying again");
                CreateIfNoDl();
            }
            Close();
        }
        string ImageToBase64()
        {
            BitmapImage image = (BitmapImage)imgChord.Source;
            var converter = new ImageSourceConverter();
            return converter.ConvertToString(image);
        }
        string FileToBase64(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var base64 = Convert.ToBase64String(bytes);
            return base64;
        }
        #endregion
    }
}
