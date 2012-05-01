using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using TestMargin.Utils;

namespace TestMargin.OverViews
{
    [Export(typeof(OvCollection))]
    class OvCollection
    {
        public event EventHandler<OvCollectionEventArgs> OvLineHovered;
        public event EventHandler<OvCollectionEventArgs> OvLineSelected; 

        public static float divHeight;      //the height of each line
        public static float widRatio;        //rate between ov and real editor
        public static float widperchar;     //width per char of real editor

        TestMargin Host { get; set; }
        public List<OvLine> _ovlc { get; set; }
        public bool IsRedraw { get; set; }

        public int SelectedLine { get; set; }

        int AuxCanvasIndexStart { get; set; }

        //maybe a work around for inter-mef call
        
        [ImportingConstructor]
        public OvCollection(TestMargin host) 
        {
            this.Host = host;
            _ovlc = new List<OvLine>();
            IsRedraw = true;
            SelectedLine = -1;

            this.Host._textView.Caret.PositionChanged += new EventHandler<Microsoft.VisualStudio.Text.Editor.CaretPositionChangedEventArgs>(Caret_PositionChanged);

            this.Host._textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_textView_LayoutChanged);

            AuxCanvasIndexStart = 0;
            //try share the same view
            //OutActor = new EditorActor(host._textView);
        }

        void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.VerticalTranslation == true && this._ovlc.Count != 0)                //scroll vertically
            {
                int lastSel = this.SelectedLine;
                if (lastSel != -1)
                    _ovlc[SelectedLine].SelectedChanged(true);
                this.SelectedLine = this.Host._tit.Scroll4OvLine();
                _ovlc[this.SelectedLine].DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, OvLine.lnStrokeTh, Brushes.DarkBlue);
            }
        }

        void Caret_PositionChanged(object sender, Microsoft.VisualStudio.Text.Editor.CaretPositionChangedEventArgs e)
        {
            //throw new NotImplementedException();
            CaretPosition cp = e.NewPosition;
            SnapshotPoint? ssp = cp.Point.GetPoint(Host._textView.TextBuffer, cp.Affinity);
            if (!ssp.HasValue) return;

            ITextSnapshotLine selectedLine = ssp.Value.GetContainingLine();

            int selectedLineNumber = selectedLine.LineNumber;

            if (this.SelectedLine != -1)
            {
                _ovlc[SelectedLine].SelectedChanged(true);

                this.SelectedLine = selectedLineNumber;
                _ovlc[this.SelectedLine].DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, OvLine.lnStrokeTh, Brushes.DarkBlue);
            }
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
            System.Diagnostics.Trace.WriteLine("-------------------------PARSELINEFOROV");
            foreach (ITextSnapshotLine tvl in Host._textView.TextSnapshot.Lines)
            {
                _ovlc.Add(new OvLine(Host, tvl, (float)(Host.ActualWidth / 4.0f),this));
            }
            AuxCanvasIndexStart = _ovlc.Count;
            //System.Diagnostics.Trace.WriteLine("###         PARSE:" + _ovlc.Count);
        }

        public void ReGenOv() 
        {
            //when to redraw the ov, when text changed or saved
            if (IsRedraw)
            {
                Parse2OvLines();
                DrawOverview();
                IsRedraw = false;
            }
        }

        public void OneSeleted(int lnnumber)
        {
            if(this.SelectedLine != -1)//selectedline == -1 -> not ready
                _ovlc[SelectedLine].SelectedChanged(true);

            int lastSel = this.SelectedLine;
            this.SelectedLine = lnnumber;

            int diff = this.SelectedLine - lastSel;

            //trigger an event
            Host._tit.Scroll4OverView(this.SelectedLine, lastSel);
        }

        public void DrawBezier(int x) 
        {

        }

    }

    class OvCollectionEventArgs : EventArgs 
    {
        public int LineSelected { get; set; }
        public int LastLineSelected { get; set; }

        public OvCollectionEventArgs(int linesel, int lastlinesel) : base() 
        {
            LineSelected = linesel;
            LastLineSelected = lastlinesel;
        }
    }
}
