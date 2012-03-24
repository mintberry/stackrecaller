using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Prototype
{
    public partial class OptionsDialog : Form
    {

        Dictionary<string, object> _objects;
        public OptionsDialog(Dictionary<string,object> objects)
        {
            _objects = objects;
   
            InitializeComponent();
            foreach (KeyValuePair<string,object> kv in _objects)
            {
                TabPage t = new TabPage();
                this.tabControl1.Controls.Add(t);
                PropertyGrid p = new PropertyGrid();
                p.SelectedObject = kv.Value;
                p.Dock = DockStyle.Fill;
                t.Controls.Add(p);
                t.Text = kv.Key;
                t.UseVisualStyleBackColor = true;
                t.Padding = new Padding(3);
            }
        }
    }
}
