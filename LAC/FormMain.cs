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
            this.textBoxOutput.VisibleChanged += new System.EventHandler(this.TextBoxOutput_VisibleChanged);

            string localFFmpeg = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "ffmpeg.exe";

            if (File.Exists(localFFmpeg))
                textBoxFFmpegPath.Text = localFFmpeg;

            textBoxOutput.Visible = false;
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

        private async void ButtonConvert_ClickAsync(object sender, EventArgs e)
        {
            if (!File.Exists(textBoxFFmpegPath.Text))
            {
                MessageBox.Show("Please check ffmpeg.exe path.", "Path to ffmpeg.exe does not exist");
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
                        Convert(sourcePath, destinationPath);
                    });

                    item.SubItems[1].Text = "Done";
                    item.Checked = false;
                    convertCount++;
                }
            }

            if (convertCount == 0)
            {
                MessageBox.Show("No files converted.", "Error");
            }
            else
            {
                MessageBox.Show(convertCount + " file(s) converted.", "Conversion complete");
            }

            buttonConvert.Enabled = true;
        }

        private int Convert(string sourcePath, string destinationPath)
        {
            Process process = new Process();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = textBoxFFmpegPath.Text;
            process.StartInfo.Arguments = "-y -vn -i \"" + sourcePath + "\" -acodec alac \"" + destinationPath + "\"";

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_OutputDataReceived);
            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(Process_ErrorDataReceived);

            process.Start();

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            return process.ExitCode;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppendConsoleText(e.Data + "\n");
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppendConsoleText(e.Data + "\n");
        }

        delegate void appendTextCallback(string text);

        private void AppendConsoleText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBoxOutput.InvokeRequired)
            {
                appendTextCallback d = new appendTextCallback(AppendConsoleText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBoxOutput.AppendText(text);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "FFmpeg Executable File|ffmpeg.exe";
            openFileDialog1.Title = "Select the FFmpeg Executable";
            openFileDialog1.FileName = "ffmpeg.exe";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxFFmpegPath.Text = openFileDialog1.FileName;
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBoxOutput.Visible = !textBoxOutput.Visible;
        }

        private void TextBoxOutput_VisibleChanged(object sender, EventArgs e)
        {
            linkLabel1.Text = textBoxOutput.Visible ? "Hide Console Output" : "Show Console Output";
        }
    }
}
