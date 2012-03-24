using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Prototype
{
    public partial class MainForm : Form
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public MainForm()
        {
            InitializeComponent();
            Model.Default.DocumentFile = "./Experiment/Sources/MonthCalendarPlusBase.cs";
        }

        private void fisheyeRenderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Model.Default.RenderStrategy = new Prototype.FisheyeRender();
        }

        private void linearRenderToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Model.Default.RenderStrategy = new Prototype.LinearRender();
        }

        void OptionsToolStripMenuItemClick(object sender, EventArgs e)
        {
            FisheyeViewOptionsWrapper options = new FisheyeViewOptionsWrapper(fisheyeView);
            Dictionary<string, object> optionObjects = new Dictionary<string, object>();
            optionObjects.Add("General options", options);
            optionObjects.Add("DOI options", Model.Default.DOIStrategy);
            (new OptionsDialog(optionObjects)).ShowDialog();
        }

        void ResetToolStripMenuItem1Click(object sender, EventArgs e)
        {
            Model.Default.Reset();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = this.ClientRectangle.Width + " X " + this.ClientRectangle.Height;
        }


        private void semanticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.Default.DOIStrategy = new StaticDOI();
         }



        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchDialog.Instance.Show();
            SearchDialog.Instance.Focus();
        }

        private void fisheyeView_Load(object sender, EventArgs e)
        {


        }

        private void optionToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                Model.Default.DocumentFile = openFileDialog1.FileName;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dynamicDOIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Model.Default.DOIStrategy = new DynamicDOI();
        }
    }
}
