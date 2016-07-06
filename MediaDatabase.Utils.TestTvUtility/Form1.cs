using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MediaDatabase.Utils.TestTvUtility
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ret = folderBrowserDialog1.ShowDialog();
            if (ret != DialogResult.OK) return;

            txtFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            var selectedPath = txtFolder.Text;

            var filenames = Directory.GetFiles(selectedPath).Select(x => new FileInfo(x).Name).ToArray();

            if (filenames.Any() && string.IsNullOrEmpty(textBox1.Text))
                textBox1.Text = filenames.First();

            listBox1.DataSource = filenames;

            listBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var ts in typeof(TvShowSpec).GetFields().Select(field => new ToolStripMenuItem(field.Name)))
            {
                ts.Click += menuItem_Click;
                contextMenuStrip1.Items.Add(ts);
            }

            foreach (var ts in typeof(TvShowSpec).GetFields().Select(field => new ToolStripMenuItem(field.Name)))
            {
                ts.Click += menuItem2_Click;
                contextMenuStrip2.Items.Add(ts);
            }

            ToolStripItem ignoreItem = new ToolStripButton("IGNORE");

            ignoreItem.Click += menuItem_Click;

            contextMenuStrip1.Items.Add(ignoreItem);

            ToolStripItem ignoreItem2 = new ToolStripButton("IGNORE");

            ignoreItem2.Click += menuItem2_Click;

            contextMenuStrip2.Items.Add(ignoreItem2);

            openFileDialog1.InitialDirectory = ConfigurationManager.AppSettings.Get("playlist.directory");
            saveFileDialog1.InitialDirectory = ConfigurationManager.AppSettings.Get("playlist.directory");
            saveFileDialog1.DefaultExt = "m3u";
            
            folderBrowserDialog1.SelectedPath = ConfigurationManager.AppSettings.Get("videos.directory");

            dataGridView1.DragEnter += playList_DragEnter;

            dataGridView1.DragDrop += playPlist_DragDrop;

            dataGridView1.AllowDrop = true;


        }

        private void playList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void playPlist_DragDrop(object sender, DragEventArgs e)
        {
            var s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            var bindingSource1 = dataGridView1.DataSource as BindingSource ?? new BindingSource();

            foreach (var entry in s.Select(t => new PlayListFile(t)))
            {
                bindingSource1.Add(new { entry.Descriptor.Name, entry.Filename });
            }

            dataGridView1.DataSource = bindingSource1;

            dataGridView1.Refresh();
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            var menuItem        = (ToolStripItem)sender;
            var newValue        = $"[{menuItem.Text}]";
            var selectionStart  = textBox1.SelectionStart;
            var selectionLength = textBox1.SelectionLength;
            try
            {
                textBox1.Text = textBox1.Text.Replace(textBox1.Text.Substring(selectionStart, selectionLength), newValue);                
                textBox1.Select(selectionStart + selectionLength, 0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        void menuItem2_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripItem)sender;
            var newValue = $"[{menuItem.Text}]";
            var selectionStart = textBox1.SelectionStart;
            var selectionLength = textBox1.SelectionLength;
            textBox1.Text = textBox1.Text.Replace(textBox1.Text.Substring(selectionStart, selectionLength), newValue);                
            textBox1.Select(selectionStart + selectionLength, 0);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var filenames = Directory.GetFiles(txtFolder.Text).Select(x => new FileInfo(x).Name).ToArray();
            var newFilenames = filenames.GetTvShowMap(textBox1.Text, textBox2.Text, "00", new Dictionary<string, string> { { "_", " " } });

            progressBar1.Minimum = 0;

            progressBar1.Maximum = newFilenames.Count;

            progressBar1.Value = 0;

            foreach (var newFilename in newFilenames)
            {
                progressBar1.Value += 1;
                try
                {
                    File.Copy(Path.Combine(txtFolder.Text, newFilename.Key), Path.Combine(txtFolder.Text, newFilename.Value));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var filenames = Directory.GetFiles(txtFolder.Text).Select(x => new FileInfo(x).Name).ToArray();
                var newFilenames = filenames.GetTvShowMap(textBox1.Text, textBox2.Text, "00", new Dictionary<string, string> { { "_", " " } });

                listBox2.DataSource = newFilenames.Values.ToArray();
                listBox2.Refresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtPlaylistPath.Text = openFileDialog1.FileName;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!File.Exists(txtPlaylistPath.Text))
                return;

            var playList = txtPlaylistPath.Text.LoadPlayList();
            var bindingSource1 = new BindingSource();
            foreach (var entry in playList.Files)
            {
                bindingSource1.Add(new {entry.Descriptor.Name, entry.Filename});
            }
            dataGridView1.DataSource = bindingSource1;
            dataGridView1.Refresh();
        }

        private void btnSavePlaylist_Click(object sender, EventArgs e)
        {
            saveFileDialog1.OverwritePrompt = true;
            
            if (!string.IsNullOrEmpty(txtPlaylistPath.Text))
            {
                saveFileDialog1.FileName = txtPlaylistPath.Text;    
            }

            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;

            new PlayList
            {
                Filename = saveFileDialog1.FileName,
                Name = saveFileDialog1.FileName,
                Files = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .Select(row => new PlayListFile(row.Cells[1].Value.ToString()))
                    .ToArray()
            }.SavePlayList();
        }

    }
}
