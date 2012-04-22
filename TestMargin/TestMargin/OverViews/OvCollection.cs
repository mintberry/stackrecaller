using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;

namespace TestMargin.OverViews
{
    class OvCollection
    {
        public static float divHeight;
        public static float widRate;
        public static float widperchar;

        TestMargin Host { get; set; }
        public List<OvLine> _ovlc { get; set; }
        public bool IsRedraw { get; set; }

        public OvCollection(TestMargin host) 
        {
            this.Host = host;
            _ovlc = new List<OvLine>();
            IsRedraw = true;
        }

        public void DrawOverview()
        {
            if (!IsRedraw) return;

            Host.Children.Clear();

            int lnCount = _ovlc.Count;
            float divHeight = (float)(Host.Height / lnCount);
            float widRate = (float)(Host.ActualWidth / 2.0f / Host._textView.ViewportWidth);
            float widperchar = TestMargin.WidthPerChar(Host._textView);
            //System.Diagnostics.Trace.WriteLine("###         DRAW:" + this.ActualWidth + " : " + widperchar);
            foreach (OvLine ovl in _ovlc)
            {
                ovl.DrawSelf(Host, widperchar, divHeight, widRate);

            }
        }


        public void Parse2OvLines()
        {
            if (_ovlc == null)
            {
                _ovlc = new List<OvLine>();
            }
            else _ovlc.Clear();

            foreach (ITextSnapshotLine tvl in Host._textView.TextSnapshot.Lines)
            {
                _ovlc.Add(new OvLine(Host, tvl, (float)(Host.ActualWidth / 4.0f)));
            }
            //System.Diagnostics.Trace.WriteLine("###         PARSE:" + _ovlc.Count);
        }
    }
}
