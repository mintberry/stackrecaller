using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Prototype
{
    public delegate bool SearchNextDelegate(string text);

    public partial class SearchDialog : Form
    {
        public SearchDialog()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Search();

        }

        private void Search()
        {
            if (!comboBox1.Items.Contains(comboBox1.Text))
                comboBox1.Items.Add(comboBox1.Text);

            if (OnSearchNext != null)
            {
                bool result = OnSearchNext(comboBox1.Text);
                label1.Visible = !result;
                if (result)
                    Logger.Default.Log(Logger.EntryType.Search, comboBox1.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        public event SearchNextDelegate OnSearchNext;

        #region SearchDialog Singleton
        public static SearchDialog Instance
        {
            get
            {
                return NestedSearchDialog.instance;
            }
        }

        class NestedSearchDialog
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedSearchDialog()
            {
            }

            internal static readonly SearchDialog instance = new SearchDialog();
        }
        #endregion



        private void SearchDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Search();
            }
            if (e.KeyCode == Keys.Escape)
                Hide();
        }

        private void SearchDialog_Activated(object sender, EventArgs e)
        {
            comboBox1.Focus();
            comboBox1.SelectAll();
        }


        
    }
}