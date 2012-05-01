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
using TestMargin.Utils;

namespace TestMargin.OverViews
{
    class OvCollection
    {
        public event EventHandler<EventArgs> OvLineHovered;
        public event EventHandler<EventArgs> OvLineSelected; 

        public static float divHeight;      //the height of each line
        public static float widRatio;        //rate between ov and real editor
        public static float widperchar;     //width per char of real editor

        TestMargin Host { get; set; }
        public List<OvLine> _ovlc { get; set; }
        public bool IsRedraw { get; set; }

        public int SelectedLine { get; set; }

        //maybe a work around for inter-mef call
        private EditorActor OutActor;

        public OvCollection(TestMargin host) 
        {
            this.Host = host;
            _ovlc = new List<OvLine>();
            IsRedraw = true;
            SelectedLine = -1;

            //try share the same view
            OutActor = new EditorActor(host._textView);
        }

        public void DrawOverview()
        {
            Host.Children.Clear();

            int lnCount = _ovlc.Count;
            divHeight = (float)(Host.Height / lnCount);
            widRatio = (float)(Host.ActualWidth / 2.0f / Host._textView.ViewportWidth);
            widperchar = TestMargin.WidthPerChar(Host._textView);
            //System.Diagnostics.Trace.WriteLine("###         DRAW:" + this.ActualWidth + " : " + widperchar);
            foreach (OvLine ovl in _ovlc)
            {
                ovl.DrawSelf(Host, widperchar, divHeight, widRatio);

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
                _ovlc.Add(new OvLine(Host, tvl, (float)(Host.ActualWidth / 4.0f),this));
            }
            //System.Diagnostics.Trace.WriteLine("###         PARSE:" + _ovlc.Count);
        }

        public void ReGenOv() 
        {
            if (IsRedraw)
            {
                Parse2OvLines();
                DrawOverview();
                IsRedraw = false;
            }
        }

        public void OneSeleted(int lnnumber)
        {
            if(this.SelectedLine != -1)
                _ovlc[SelectedLine].SelectedChanged(true);

            int lastSel = this.SelectedLine;
            this.SelectedLine = lnnumber;

            int diff = this.SelectedLine - lastSel;

            OutActor.ScrollLines(this.SelectedLine, diff);
            //trigger an event
        }

        public void DrawBezier(int x) 
        {

        }
    }
}
