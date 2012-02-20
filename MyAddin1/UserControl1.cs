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
using Microsoft.VisualStudio.CommandBars;

namespace MyAddin1
{
    public partial class AboutBochs : Form
    {
        public AboutBochs()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }

        public DTE2 DTEObject {
            set { _applicationObject = value; }
            get { return _applicationObject; }
        }

        public DialogResult testDialog() {
            this.label1.Text = "copyright (c) 2012 Ne Qi";
            BindCommandBars();
            return ShowDialog();
        }

        private DTE2 _applicationObject;

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BindCommandBars()
        {
            CommandBars cmds = _applicationObject.CommandBars as CommandBars;

            #region SortedNames

            List<string> names = new List<string>(cmds.Count);
            foreach (CommandBar bar in cmds)
            {
                names.Add(bar.Name.Replace("&",""));
            }

            names.Sort();
            foreach (string name in names)
            {
                TreeNode node = new TreeNode();
                node.Text = name;//.Replace("&","");
                node.Tag = "bar";
                // Add a dummy node
                node.Nodes.Add("dummyNode");

                treeView1.Nodes.Add(node);
            }

            #endregion
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            string nodeType = node.Tag as string;

            if (nodeType == "bar")
            {
                FillBar(node);
            }
            else if (nodeType == "popup")
            {
                FillPopup(node);
            }
        }
        private void FillBar(TreeNode node) 
        {
            node.Nodes.Clear();
            CommandBar bar = (_applicationObject.CommandBars as CommandBars)[node.Text];
            foreach(CommandBarControl cmdctrl in bar.Controls)
            {
                if (!string.IsNullOrEmpty(cmdctrl.Caption)) 
                {
                    TreeNode newnode = new TreeNode();
                    newnode.Text = cmdctrl.Caption.Replace("&","");

                    if (cmdctrl is CommandBarPopup)
                    {
                        newnode.Tag = "popup";
                        newnode.Nodes.Add("dummyNode");
                    }
                    node.Nodes.Add(newnode);
                }
                
            }


        }
        private void FillPopup(TreeNode node) 
        {
            node.Nodes.Clear();
            CommandBar bar = (_applicationObject.CommandBars as CommandBars)[node.Parent.Text];
            CommandBarPopup popup = bar.Controls[node.Text] as CommandBarPopup;

            foreach(CommandBarControl cmdctrl in popup.Controls)
            {
                if (!string.IsNullOrEmpty(cmdctrl.Caption))
                {
                    TreeNode newnode = new TreeNode();
                    newnode.Text = cmdctrl.Caption.Replace("&","");
                    node.Nodes.Add(newnode);
                }
            }

        }
        
    }

}