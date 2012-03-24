using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Prototype
{
    public partial class ScrollableDX : UserControl
    {
        public DXLayoutControl CodeView
        {
            get { return this.dxLayoutControl1; }
        }

        protected override void OnResize(EventArgs e)
        {
            hScrollBar1.Maximum = dxLayoutControl1.ScrollableWidth;
            hScrollBar1.LargeChange = dxLayoutControl1.ScrollableArea;
            hScrollBar1.Minimum = 0;
            hScrollBar1.Value = 0;
            base.OnResize(e);
        }

        public int ScroolBarHeight
        {
            get { return hScrollBar1.Height; }
        }

        public ScrollableDX()
        {
            InitializeComponent();
            hScrollBar1.Scroll += new ScrollEventHandler(hScrollBar1_Scroll);

        }

        void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            dxLayoutControl1.HScrollValue = hScrollBar1.Value;
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            //dxLayoutControl1.HScrollValue = hScrollBar1.Value;
        }
    }
}
