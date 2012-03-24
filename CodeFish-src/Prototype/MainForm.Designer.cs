/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 19-02-2007
 * Time: 12:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Prototype
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.fisheyeView = new Prototype.CodeviewControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.strategiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fisheyeRenderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.linearRenderToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.semanticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dynamicDOIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.simpleFocusToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.experimentPanel1 = new Prototype.Experiment.ExperimentPanel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 681);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1028, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(61, 17);
            this.toolStripStatusLabel2.Text = "Resolution:";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(13, 17);
            this.toolStripStatusLabel3.Text = "0";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.fisheyeView);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(803, 651);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(222, 3);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(803, 675);
            this.toolStripContainer1.TabIndex = 3;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // fisheyeView
            // 
            this.fisheyeView.BackColor = System.Drawing.SystemColors.Window;
            this.fisheyeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fisheyeView.Location = new System.Drawing.Point(0, 0);
            this.fisheyeView.Margin = new System.Windows.Forms.Padding(4);
            this.fisheyeView.Name = "fisheyeView";
            this.fisheyeView.Size = new System.Drawing.Size(803, 651);
            this.fisheyeView.TabIndex = 0;
            this.fisheyeView.Load += new System.EventHandler(this.fisheyeView_Load);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem1,
            this.resetToolStripMenuItem,
            this.searchToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(803, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // optionToolStripMenuItem
            // 
            this.optionToolStripMenuItem.Image = global::Prototype.Properties.Resources.page_white_text;
            this.optionToolStripMenuItem.Name = "optionToolStripMenuItem";
            this.optionToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.optionToolStripMenuItem.Text = "Load file";
            this.optionToolStripMenuItem.Click += new System.EventHandler(this.optionToolStripMenuItem_Click_1);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::Prototype.Properties.Resources.cancel;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.resetToolStripMenuItem1,
            this.strategiesToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "Edit";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Image = global::Prototype.Properties.Resources.application_form_edit;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OptionsToolStripMenuItemClick);
            // 
            // resetToolStripMenuItem1
            // 
            this.resetToolStripMenuItem1.Image = global::Prototype.Properties.Resources.arrow_rotate_clockwise;
            this.resetToolStripMenuItem1.Name = "resetToolStripMenuItem1";
            this.resetToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.resetToolStripMenuItem1.Text = "Reset";
            this.resetToolStripMenuItem1.Click += new System.EventHandler(this.ResetToolStripMenuItem1Click);
            // 
            // strategiesToolStripMenuItem
            // 
            this.strategiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fisheyeRenderToolStripMenuItem1,
            this.linearRenderToolStripMenuItem1,
            this.toolStripSeparator3,
            this.semanticToolStripMenuItem,
            this.dynamicDOIToolStripMenuItem,
            this.toolStripSeparator4,
            this.simpleFocusToolStripMenuItem1});
            this.strategiesToolStripMenuItem.Image = global::Prototype.Properties.Resources.arrow_branch;
            this.strategiesToolStripMenuItem.Name = "strategiesToolStripMenuItem";
            this.strategiesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.strategiesToolStripMenuItem.Text = "Strategies";
            // 
            // fisheyeRenderToolStripMenuItem1
            // 
            this.fisheyeRenderToolStripMenuItem1.Image = global::Prototype.Properties.Resources.page_paintbrush;
            this.fisheyeRenderToolStripMenuItem1.Name = "fisheyeRenderToolStripMenuItem1";
            this.fisheyeRenderToolStripMenuItem1.Size = new System.Drawing.Size(157, 22);
            this.fisheyeRenderToolStripMenuItem1.Text = "Fisheye render";
            this.fisheyeRenderToolStripMenuItem1.Click += new System.EventHandler(this.fisheyeRenderToolStripMenuItem1_Click);
            // 
            // linearRenderToolStripMenuItem1
            // 
            this.linearRenderToolStripMenuItem1.Image = global::Prototype.Properties.Resources.page_paintbrush;
            this.linearRenderToolStripMenuItem1.Name = "linearRenderToolStripMenuItem1";
            this.linearRenderToolStripMenuItem1.Size = new System.Drawing.Size(157, 22);
            this.linearRenderToolStripMenuItem1.Text = "Linear render";
            this.linearRenderToolStripMenuItem1.Click += new System.EventHandler(this.linearRenderToolStripMenuItem1_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(154, 6);
            // 
            // semanticToolStripMenuItem
            // 
            this.semanticToolStripMenuItem.Image = global::Prototype.Properties.Resources.page_code;
            this.semanticToolStripMenuItem.Name = "semanticToolStripMenuItem";
            this.semanticToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.semanticToolStripMenuItem.Text = "Semantic DOI";
            this.semanticToolStripMenuItem.Click += new System.EventHandler(this.semanticToolStripMenuItem_Click);
            // 
            // dynamicDOIToolStripMenuItem
            // 
            this.dynamicDOIToolStripMenuItem.Image = global::Prototype.Properties.Resources.page_lightning;
            this.dynamicDOIToolStripMenuItem.Name = "dynamicDOIToolStripMenuItem";
            this.dynamicDOIToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.dynamicDOIToolStripMenuItem.Text = "Dynamic DOI";
            this.dynamicDOIToolStripMenuItem.Click += new System.EventHandler(this.dynamicDOIToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(154, 6);
            this.toolStripSeparator4.Visible = false;
            // 
            // simpleFocusToolStripMenuItem1
            // 
            this.simpleFocusToolStripMenuItem1.Image = global::Prototype.Properties.Resources.page_green;
            this.simpleFocusToolStripMenuItem1.Name = "simpleFocusToolStripMenuItem1";
            this.simpleFocusToolStripMenuItem1.Size = new System.Drawing.Size(157, 22);
            this.simpleFocusToolStripMenuItem1.Text = "Simple focus";
            this.simpleFocusToolStripMenuItem1.Visible = false;
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(12, 20);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.Image = global::Prototype.Properties.Resources.find;
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.ShortcutKeyDisplayString = "CTRl + F";
            this.searchToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.searchToolStripMenuItem.Text = "Find";
            this.searchToolStripMenuItem.Click += new System.EventHandler(this.searchToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.38728F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.61272F));
            this.tableLayoutPanel1.Controls.Add(this.experimentPanel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.toolStripContainer1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1028, 681);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // experimentPanel1
            // 
            this.experimentPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.experimentPanel1.Location = new System.Drawing.Point(3, 3);
            this.experimentPanel1.Name = "experimentPanel1";
            this.experimentPanel1.Size = new System.Drawing.Size(213, 675);
            this.experimentPanel1.TabIndex = 1;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "C# files|*.cs";
            this.openFileDialog1.Title = "Load source code file";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 703);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "CodeFish, User study of fisheye visualization of source code";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private Prototype.CodeviewControl fisheyeView;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem strategiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fisheyeRenderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem linearRenderToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem semanticToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem simpleFocusToolStripMenuItem1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Prototype.Experiment.ExperimentPanel experimentPanel1;
        private System.Windows.Forms.ToolStripMenuItem dynamicDOIToolStripMenuItem;
	}
}
