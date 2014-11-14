using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CodeBuilder
{
    public partial class Form1 : Form
    {
        private CreateCodeAndSave codeManager;
        public Form1()
        {
            codeManager = new CreateCodeAndSave();
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            codeManager.DBPath = this.textBox1.Text;
            codeManager.ResultPath = this.textBox2.Text;
            codeManager.start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string resultFile = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "D:";
            openFileDialog1.Filter = "All files (*.*)|*.*|Sqlite files (*.db)|*.db";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                resultFile = openFileDialog1.FileName;
            this.textBox1.Text = resultFile;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string resultFile = "";
            FolderBrowserDialog FolderFileDialog1 = new FolderBrowserDialog();
            FolderFileDialog1.SelectedPath = "D:";
            if (FolderFileDialog1.ShowDialog() == DialogResult.OK)
                resultFile = FolderFileDialog1.SelectedPath;
            this.textBox2.Text = resultFile;
        }
    }
}
