﻿using System;
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
        public static float divHeight;      //the height of each line
        public static float widRatio;        //rate between ov and real editor
        public static float widperchar;     //width per char of real editor

        TestMargin Host { get; set; }
        public List<OvLine> _ovlc { get; set; }
        public bool IsRedraw { get; set; }

        public int SelectedLine { get; set; }

        public OvCollection(TestMargin host) 
        {
            this.Host = host;
            _ovlc = new List<OvLine>();
            IsRedraw = true;
            SelectedLine = -1;
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
                _ovlc.Add(new OvLine(Host, tvl, (float)(Host.ActualWidth / 4.0f)));
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

        public void DrawBezier() 
        {

        }
    }
}
