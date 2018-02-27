using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LAC
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(FormMain_DragEnter);
            this.DragDrop += new DragEventHandler(FormMain_DragDrop);

            string localFFmpeg = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "ffmpeg.exe";

            if (File.Exists(localFFmpeg))
                textBoxFFmpegPath.Text = localFFmpeg;
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filePath in filePaths)
            {
                if (filePath.ToLower().EndsWith(".flac"))
                {
                    ListViewItem foundItem = listViewFiles.FindItemWithText(filePath);
                    if (foundItem == null)
                    {
                        string[] arr = new string[2];
                        arr[0] = filePath;
                        arr[1] = "Ready";
                        ListViewItem listViewItem = new ListViewItem(arr);
                        listViewItem.Checked = true;
                        listViewFiles.Items.Add(listViewItem);
                    }
                } else
                {
                    Console.WriteLine(filePath + " does not have a FLAC extension.");
                }
                
            }
        }

        private async void buttonConvert_ClickAsync(object sender, EventArgs e)
        {
            if(!File.Exists(textBoxFFmpegPath.Text))
            {
                MessageBox.Show("Please check ffmpeg.exe path.");
                return;
            }

            buttonConvert.Enabled = false;
            int convertCount = 0;

            foreach (ListViewItem item in listViewFiles.Items)
            {
                if (item.Checked)
                {
                    item.SubItems[1].Text = "Converting...";
                    string sourcePath = item.SubItems[0].Text;
                    string destinationPath = Path.GetDirectoryName(sourcePath) + 
                        Path.DirectorySeparatorChar + 
                        Path.GetFileNameWithoutExtension(sourcePath) + 
                        ".m4a";

                    await Task.Run(() =>
                    {
                        convert(sourcePath, destinationPath);
                    });

                    item.SubItems[1].Text = "Done";
                    item.Checked = false;
                    convertCount++;
                }
            }

            if(convertCount == 0)
            {
                MessageBox.Show("No files converted.");
            } else
            {
                MessageBox.Show(convertCount + " files converted.");
            }

            buttonConvert.Enabled = true;
        }

        private void convert(string sourcePath, string destinationPath)
        {
            Process process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = textBoxFFmpegPath.Text;
            process.StartInfo.Arguments = "-y -vn -i \"" + sourcePath + "\" -acodec alac \"" + destinationPath + "\"";
            
            process.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "FFmpeg Executable File|ffmpeg.exe";
            openFileDialog1.Title = "Select the FFmpeg Executable";
            openFileDialog1.FileName = "ffmpeg.exe";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // put path into box
                textBoxFFmpegPath.Text = openFileDialog1.FileName;
            }
        }
    }
}
