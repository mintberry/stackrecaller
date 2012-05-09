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
using Microsoft.VisualStudio.Text.Formatting;
using TestMargin.Utils;
using TestMargin.Taggers;

namespace TestMargin.OverViews
{
    enum TriBezierLines {//three bezier lines
        Top,
        Mid,
        Bot,
        Hover
    };

    [Export(typeof(OvCollection))]
    class OvCollection
    {
        public event EventHandler<OvCollectionEventArgs> OvLineHovered;
        public event EventHandler<OvCollectionEventArgs> OvLineSelected; 

        public static float divHeight;      //the height of each line
        public static float widRatio;        //rate between ov and real editor
        public static float widperchar;     //width per char of real editor
        public static int  ipartial = 1;     //width per char of real editor

        public static float dh_threshold = 1.8f;   //maximum divheight

        TestMargin Host { get; set; }
        public List<OvLine> _ovlc { get; set; }
        public bool IsRedraw { get; set; }

        public int SelectedLine { get; set; }

        public int AuxCanvasIndexStart { get; set; }

        public BezierLine [] bzLines;

        public BezierLine HoverBezier;

        //maybe a work around for inter-mef call
        
        [ImportingConstructor]
        public OvCollection(TestMargin host) 
        {
            this.Host = host;
            _ovlc = new List<OvLine>();
            IsRedraw = true;
            SelectedLine = -1;

            this.Host._textView.Caret.PositionChanged += new EventHandler<Microsoft.VisualStudio.Text.Editor.CaretPositionChangedEventArgs>(Caret_PositionChanged);
            this.Host._tit.ScrollNumberFixed += new EventHandler<TextViewLayoutChangedEventArgs>(_tit_ScrollNumberFixed);

            this.Host._textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_textView_LayoutChanged);
            this.Host._tit.FocusAreaTriggerBezier += new EventHandler<TriggerBezierEventArgs>(_tit_FocusAreaTriggerBezier);

            AuxCanvasIndexStart = 0;

            bzLines = new BezierLine[3];

            bzLines[TriBezierLines.Top - TriBezierLines.Top] = new BezierLine(TriBezierLines.Top);
            bzLines[TriBezierLines.Mid - TriBezierLines.Top] = new BezierLine(TriBezierLines.Mid);
            bzLines[TriBezierLines.Bot - TriBezierLines.Top] = new BezierLine(TriBezierLines.Bot);

            HoverBezier = new BezierLine(TriBezierLines.Hover);
            //try share the same view
            //OutActor = new EditorActor(host._textView);
        }

        void _tit_FocusAreaTriggerBezier(object sender, TriggerBezierEventArgs e)
        {
            if (e != null)
            {
                int hl = e.HoveredLine;
                DrawHoverBezier(hl);
            }
            //do bezier redraw only
            else
                ReGenBezier();
        }

        void _tit_ScrollNumberFixed(object sender, TextViewLayoutChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e == null)
            {
                int selectedLineNumber = Host._tit.GetCentralLine4Ov();

                if (this.SelectedLine != -1 && selectedLineNumber != this.SelectedLine)
                {
                    _ovlc[SelectedLine].SelectedChanged(true);

                    this.SelectedLine = selectedLineNumber;
                    _ovlc[this.SelectedLine].DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, OvLine.lnStrokeTh, Brushes.DarkBlue);

                    ReGenBezier();
                }
            }
        }

        void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.VerticalTranslation == true && this._ovlc.Count != 0)                //scroll vertically
            {
                int lastSel = this.SelectedLine;
                if (lastSel != -1)
                    _ovlc[SelectedLine].SelectedChanged(true);
                this.SelectedLine = this.Host._tit.GetCentralLine4Ov();
                if (this.SelectedLine == -1)
                    return;
                _ovlc[this.SelectedLine].DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, OvLine.lnStrokeTh, Brushes.DarkBlue);

                ReGenBezier();
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

                ReGenBezier();
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
            float partial = 0.0f;
            if (divHeight < 1.0)
            {
                partial = (float)(1.0 / divHeight);
                ipartial = (int)Math.Ceiling(partial);
            }
            else
                divHeight = (float)OvLine.GetDivHeight(divHeight);

            foreach (OvLine ovl in _ovlc)
            {
                //for those too many lines, only draw one for several
                if (ipartial > 1)
                {
                    if (ovl.lnNumber % ipartial == 0)
                        ovl.DrawSelf(Host, widperchar, divHeight /** ipartial*/, widRatio);
                    continue;
                }
                else 
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
            //System.Diagnostics.Trace.WriteLine("-------------------------PARSELINEFOROV");
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
                //emuParser.ReCalFocusAreaHeight(Host._textView);

                ipartial = 1;
                Parse2OvLines();
                DrawOverview();

                ReGenBezier(true);

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
            Host._tit.GenSelected();
        }

        public bool GenBezierLine()
        {
            if (_ovlc.Count > 0)
            {
                int centralLine = Host._tit.GetCentralLine4Ov();

                if (centralLine == -1 || centralLine == (_ovlc.Count - 1))
                    return false;
                //ITextViewLine topline = EditorActor.GetSpecViewLine(Host._textView, emuParser.central_offset, TriBezierLines.Top);
                ITextViewLine midline = EditorActor.GetTopLine(Host._textView, centralLine, 1);//get the centralline
                //ITextViewLine botline = EditorActor.GetSpecViewLine(Host._textView, emuParser.central_offset, TriBezierLines.Bot);
                if (midline == null)
                {
                    return false;
                }
                int topindex = centralLine - emuParser.central_offset + 1;
                int botindex = centralLine + emuParser.central_offset - 1;
                double lineHeight = midline.Height;
                double topTop = midline.Top - lineHeight * (emuParser.central_offset - 1);
                bzLines[TriBezierLines.Top - TriBezierLines.Top].leftPointY = topTop >= 0.0 ? topTop : 0.0;
                bzLines[TriBezierLines.Mid - TriBezierLines.Top].leftPointY = midline.Top + lineHeight / 2.0;
                bzLines[TriBezierLines.Bot - TriBezierLines.Top].leftPointY = midline.Top + lineHeight * emuParser.central_offset;
                
                bzLines[TriBezierLines.Top - TriBezierLines.Top].RightOvLine = _ovlc[topindex >= 0?topindex:0];
                bzLines[TriBezierLines.Mid - TriBezierLines.Top].RightOvLine = _ovlc[centralLine];
                bzLines[TriBezierLines.Bot - TriBezierLines.Top].RightOvLine = _ovlc[botindex >= _ovlc.Count ? _ovlc.Count - 1 : botindex];

                HoverBezier.leftPointY = bzLines[TriBezierLines.Mid - TriBezierLines.Top].leftPointY;
                HoverBezier.RightOvLine = _ovlc[centralLine]; ;

                return true;
                //System.Diagnostics.Trace.WriteLine("------POS:" + bzLines[TriBezierLines.Top - TriBezierLines.Top].LeftTvLine.Top);
            }
            return false;
        }

        public void DrawBezier(bool IsFirstdraw, int x = 0) 
        {
            if (_ovlc.Count > 0)
            {
                foreach (BezierLine bzLine in bzLines)
                {
                    bzLine.DrawSelf(Host, IsFirstdraw);
                }
                //HoverBezier.DrawSelf(Host, true);
            }
        }
        void ReGenBezier(bool IsFirstdraw = false) 
        {
            if(GenBezierLine())
                DrawBezier(IsFirstdraw);
        }

        public void DrawHoverBezier(int hoverline)
        {
            ITextViewLine hline = EditorActor.GetTopLine(Host._textView, hoverline, 1);
            if (hline == null)
            {
                return;
            }
            double lineHeight = hline.Height;
            HoverBezier.leftPointY = hline.Top + lineHeight / 2.0;
            HoverBezier.RightOvLine = _ovlc[hoverline];
            HoverBezier.DrawSelf(Host, true);
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
