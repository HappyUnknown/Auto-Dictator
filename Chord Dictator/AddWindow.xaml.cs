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
        string audiopath = "App Files/Audio/";
        string imagepath = "App Files/Images/";
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
            File.AppendAllText(log, "-> " + message);
            if (exmsg.Length > 0) File.AppendAllText(log, " (" + exmsg + ") ");
            if (tip.Length > 0) File.AppendAllText(log, "[Tip: " + tip.TrimEnd('.') + "]");
            File.AppendAllText(log, Environment.NewLine);
        }
        bool FileOk(bool writelog = false)
        {
            string[] filelines = File.ReadAllLines(dfp);
            foreach (string line in filelines)
                if (line.Split('>').Length != 3 && line.Length > 0)
                {
                    if (writelog) WriteToLog("Row must contain 2 '>' [Path: " + dfp + "]");
                    return false;
                }
            return true;
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
                    imgElement.Source = new BitmapImage(new Uri(ofd.FileName));
                    tbElementImgPath.Text = ofd.FileName;
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
                if (path[i] == '/')
                {
                    return path.Substring(0, i) + "/";
                }
            }
            return "NO_PATH";
        }
        private void btnConnSound_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "MP3 Sound|*.mp3|WAV Sound|*.wav|All|*.*";
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
        private void btnAddElement_Click(object sender, RoutedEventArgs e)
        {
            FileOk(true);
            AddElement(tbName.Text.Trim(' '), tbSoundPath.Text, tbElementImgPath.Text);
        }
        private void btnAddMoveElement_Click(object sender, RoutedEventArgs e)
        {
            string aupath = "-";
            string impath = "-";
            try
            {
                aupath = Environment.CurrentDirectory + "\\" + audiopath + GetFileName(tbSoundPath.Text);
                impath = Environment.CurrentDirectory + "\\" + imagepath + GetFileName(tbElementImgPath.Text);
                try
                {
                    if (!File.Exists(aupath) && tbSoundPath.Text.Length > 2) File.Copy(tbSoundPath.Text, aupath); else { WriteToLog("File chosen was located in root folder, so it wasn't copied. [Path: " + tbSoundPath.Text + "]"); }
                    if (!File.Exists(impath) && tbSoundPath.Text.Length > 2) File.Copy(tbElementImgPath.Text, impath); else { WriteToLog("File chosen was located in root folder, so it wasn't copied. [Path: " + tbElementImgPath.Text + "]"); }
                    AddElement(tbName.Text, aupath, impath);
                }
                catch (Exception ex)
                {
                    WriteToLog("Troubles during moving files to program folder.", "btnAddMoveElement_Click", ex.Message);
                    MessageBox.Show("Error during add operation (check logs).");
                }
                WriteToLog("New image path (copy) : " + aupath, "btnAddMoveElement_Click");
                WriteToLog("New audio path (copy) : " + impath, "btnAddMoveElement_Click");
            }
            catch (Exception ex)
            {
                WriteToLog("Troubles during building new paths.(" + aupath + "," + impath + ")", "btnAddMoveElement_Click", ex.Message);
                MessageBox.Show("Error during add operation (check logs).");
            }

        }
        void AddElement(string name, string Elementpath, string imagepath)
        {
            try
            {
                if (name == "")
                {
                    MessageBox.Show("No name signed. Rename please.");
                    return;
                }
                if (Elementpath == "")
                {
                    Elementpath = defaultAudio;
                    MessageBox.Show("No sound signed.");
                }
                if (imgElement.Source == null || tbElementImgPath.Text == "")
                {
                    imagepath = defaultImage;
                    MessageBox.Show("No image signed.");
                }

                if (!name.Contains('>') && !imagepath.Contains('>') && !Elementpath.Contains('>'))
                {
                    File.AppendAllText(dfp, name + ">" + imagepath + ">" + Elementpath + Environment.NewLine);
                    MessageBox.Show("Added \"" + name + "\" element. Path: " + dfp);
                    imgElement.Source = null;
                    tbName.Text = string.Empty;
                    tbSoundPath.Text = string.Empty;
                    tbElementImgPath.Text = string.Empty;
                    WriteToLog("Added new element \"" + name + "\".", "AddElement");
                }
                else
                {
                    MessageBox.Show("\">\" program symbol used. Try removing it.");
                    WriteToLog("Program symbol used in dictionary addition.", "AddElement");
                }
                Close();
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add Element.", "AddElement", ex.Message, "Try recreating initial dictionary or trying again");
                CreateIfNoDl();
            }
        }
        private void btnRmLast_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                FileOk(true);

                if (MessageBox.Show("Do you want to remove last element?", "Removing last", MessageBoxButton.YesNo) == MessageBoxResult.Yes && File.ReadAllLines(dfp).Length > 0)
                {
                    List<string> content = File.ReadAllLines(dfp).ToList();
                    MessageBox.Show("Last element removed. - " + content[content.Count - 1].Split('>')[0]);
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
        void AddElementPortable()
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
                if (imgElement.Source == null || tbElementImgPath.Text == "")
                {
                    MessageBox.Show("No image signed.");
                    return;
                }
                else
                {
                    if (!tbName.Text.Contains('>'))
                    {
                        File.AppendAllText(dfp, tbName.Text + ">" + FileToBase64(tbElementImgPath.Text) + ">" + FileToBase64(tbSoundPath.Text) + Environment.NewLine);
                        MessageBox.Show("Added \"" + tbName.Text + "\" Element.");
                        imgElement.Source = null;
                        tbName.Text = string.Empty;
                        tbSoundPath.Text = string.Empty;
                        tbElementImgPath.Text = string.Empty;
                    }
                    else
                    {
                        WriteToLog("Failed to add element due to \">\" was used.", "AddElementPortable");
                        MessageBox.Show("\">\" program symbol used. Try removing it.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog("Failed to add Element.", "AddElementPortable", ex.Message, "Try recreating initial dictionary or trying again");
                CreateIfNoDl();
            }
            Close();
        }
        string ImageToBase64()
        {
            BitmapImage image = (BitmapImage)imgElement.Source;
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
