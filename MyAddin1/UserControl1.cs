using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Extensibility;

namespace MyAddin1
{
    public partial class AboutBochs : Form
    {
        public AboutBochs()
        {
            InitializeComponent();
            
        }

        public DTE2 DTEObject {
            set { _applicationObject = value; }
        }

        public DialogResult testDialog() {
            this.label1.Text = _applicationObject.Commands.ToString();
            return ShowDialog();
        }

        private DTE2 _applicationObject;
    }

}