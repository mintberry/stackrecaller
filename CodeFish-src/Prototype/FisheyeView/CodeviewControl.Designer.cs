/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 19-02-2007
 * Time: 12:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace Prototype
{
	partial class CodeviewControl
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.scrollableDX = new Prototype.ScrollableDX();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // scrollableDX
            // 
            this.scrollableDX.BackColor = System.Drawing.Color.White;
            this.scrollableDX.Location = new System.Drawing.Point(0, 0);
            this.scrollableDX.Name = "scrollableDX";
            this.scrollableDX.Size = new System.Drawing.Size(225, 213);
            this.scrollableDX.TabIndex = 0;
            // 
            // FisheyeView
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.scrollableDX);
            this.Name = "FisheyeView";
            this.Size = new System.Drawing.Size(249, 213);
            this.ResumeLayout(false);

        }

        private Prototype.ScrollableDX scrollableDX;
        private System.Windows.Forms.Timer timer1;
	}
}
