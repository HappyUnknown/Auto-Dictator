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
            WriteToLog("Editing started.", "AddWindowConstructor");
        }
        void WriteToLog(string message, string functionName = "", string exmsg = "", string tip = "")
        {
            if (!File.Exists(log)) File.Create(log).Close();
            File.AppendAllText(log, "[" + DateTime.Now + "] ");
            if (functionName.Length > 0) File.AppendAllText(log, functionName + "()");
            File.AppendAllText(log, " -> " + message);
            if (exmsg.Length > 0) File.AppendAllText(log, " (" + exmsg + ") ");
            if (tip.Length > 0) File.AppendAllText(log, "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText(log, Environment.NewLine);
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
                WriteToLog("Initial dictionary file created.", "CreateIfNoDl");
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
                WriteToLog("Failed to connect image.", "btnConnImg_Click", ex.Message);
            }
        }
        string GetFileName(string path)
        {
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '\\')
                {
                    return path.Substring(i + 1, path.Length - i - 1);
                }
            }
            return "NO_NAME";
        }
        string GetFileHome(string path)
        {
            for (int i = path.Length - 1; i >= 0; i--)
            {
                MessageBox.Show(path[i].ToString());
                if (path[i] == '/')
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
                MessageBox.Show("Error during add operation (check logs).");
                File.AppendAllText(log, "Failed to connect sound: " + DateTime.Now + "-> " + ex.Message + Environment.NewLine);
            }
        }
        private void btnAddChord_Click(object sender, RoutedEventArgs e)
        {
            AddChord(tbName.Text, tbSoundPath.Text, tbChordImgPath.Text);
        }
        private void btnAddMoveChord_Click(object sender, RoutedEventArgs e)
        {
            string aupath = "-";
            string impath = "-";
            try
            {
                aupath = GetFileHome(defaultAudio) + GetFileName(tbSoundPath.Text);
                impath = GetFileHome(defaultImage) + GetFileName(tbChordImgPath.Text);
                try
                {
                    File.Copy(tbSoundPath.Text, aupath);
                    File.Copy(tbChordImgPath.Text, impath);
                    AddChord(tbName.Text, aupath, impath);
                }
                catch (Exception ex)
                {
                    WriteToLog("Troubles during moving files to program folder.", "btnAddMoveChord_Click", ex.Message);
                    MessageBox.Show("Error during add operation (check logs).");
                }
                WriteToLog("New image path (copy) : " + aupath, "btnAddMoveChord_Click");
                WriteToLog("New audio path (copy) : " + impath, "btnAddMoveChord_Click");
            }
            catch (Exception ex)
            {
                WriteToLog("Troubles during building new paths.(" + aupath + "," + impath + ")", "btnAddMoveChord_Click", ex.Message);
                MessageBox.Show("Error during add operation (check logs).");
            }

        }
        void AddChord(string name, string chordpath, string imagepath)
        {
            try
            {
                if (name == "")
                {
                    MessageBox.Show("No name signed. Rename please.");
                    return;
                }
                if (chordpath == "")
                {
                    chordpath = defaultAudio;
                    MessageBox.Show("No sound signed.");
                }
                if (imgChord.Source == null || tbChordImgPath.Text == "")
                {
                    imagepath = defaultImage;
                    MessageBox.Show("No image signed.");
                }

                if (!name.Contains('>') && !imagepath.Contains('>') && !chordpath.Contains('>'))
                {
                    File.AppendAllText(dfp, name + ">" + imagepath + ">" + chordpath + Environment.NewLine);
                    MessageBox.Show("Added \"" + name + "\" element. Path: " + dfp);
                    imgChord.Source = null;
                    tbName.Text = string.Empty;
                    tbSoundPath.Text = string.Empty;
                    tbChordImgPath.Text = string.Empty;
                    WriteToLog("Added new element \"" + name + "\".", "AddChord");
                }
                else
                {
                    MessageBox.Show("\">\" program symbol used. Try removing it.");
                    WriteToLog("Program symbol used in dictionary addition.", "AddChord");
                }
                Close();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add chord.", "AddChord", ex.Message, "Try recreating initial dictionary or trying again");
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
                WriteToLog("Failed to remove last from dictionary.", "btnRmLast_Click", ex.Message, "Your dictionary file was recreated, because program did not reach it. Try again.");
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
                        WriteToLog("Failed to add chord due to \">\" was used.", "AddChordPortable");
                        MessageBox.Show("\">\" program symbol used. Try removing it.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add chord.", "AddChordPortable", ex.Message, "Try recreating initial dictionary or trying again");
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
