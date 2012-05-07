using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using TestMargin.OverViews;

namespace TestMargin.Utils
{
    /// <summary>
    /// control the editor scrolling actions
    /// </summary>
    class EditorActor
    {
        ITextView View { get; set; }                                      //iwpftextview
        public int CentralLine { get; set; }
        public int SelectedLine { get; set; }                             //
        public int HoverLine { get; set; }
        public IViewScroller Scroller { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public EditorActor(ITextView itv)
        {
            this.View = itv;
            Scroller = View.ViewScroller;

            GetCentralLine();
            HoverLine = -1;
            SelectedLine = -1;
        }



        #region Helpers

        public int ScrollLines(int targetLineNumber, int lineNumbers)
        {
            this.GetCentralLine();
            if (-1 == this.CentralLine)
                return -1;
            ScrollDirection direction = lineNumbers > 0 ? ScrollDirection.Down : ScrollDirection.Up;
            this.Scroller.ScrollViewportVerticallyByLines(direction, Math.Abs(lineNumbers));

            //System.Diagnostics.Trace.WriteLine("^^^                 CENTRAL: " + this.CentralLine);

            if (this.GetCentralLine() != targetLineNumber && !IsEdge(0) && IsEdge(View.TextSnapshot.LineCount - 1))
            {
                //better not use recursion here
                //ScrollLines(targetLineNumber, targetLineNumber - this.CentralLine);
                lineNumbers = targetLineNumber - this.CentralLine;
                ScrollDirection direction_r = lineNumbers > 0 ? ScrollDirection.Down : ScrollDirection.Up;
                this.Scroller.ScrollViewportVerticallyByLines(direction_r, Math.Abs(lineNumbers));

                return 1;
            }
            return 0;
        }

        /// <summary>subject to change
        /// </summary>
        /// <returns>central viewline number</returns>
        public int GetCentralLine()
        {
                return CentralLine = this.subGetCentralLine();
        }

        /// <summary>subject to change
        /// </summary>
        /// <returns>central viewline number</returns>
        public int subGetCentralLine()
        {
            if (View == null)
            {
                return -1;
            }
            ITextViewLineCollection tvlc;
            try
            {
                tvlc = View.TextViewLines;
            }
            catch (InvalidOperationException)
            {
                return -1;
            }
            if (tvlc == null) return -1;                         //quite important, not applicable now
            int colSize = tvlc.Count;
            //return CentralLine = TestMargin.GetViewLineNumber(tvlc[colSize / 2]);
            try
            {
                double vpCentral = View.ViewportHeight / 2.0;
                double llCentral = tvlc.LastVisibleLine.Bottom / 2.0;
                return TestMargin.GetViewLineNumber(tvlc.GetTextViewLineContainingYCoordinate(Math.Min(vpCentral,llCentral)));
            }
            catch
            {
                return -1;
            }
        }

        bool IsEdge(int toporbot)
        {
            ITextViewLineCollection tvlc = View.TextViewLines;
            if (tvlc == null) return true;                         //quite important

            int colSize = tvlc.Count;
            return TestMargin.GetViewLineNumber(tvlc.FirstVisibleLine) == toporbot;
        }

        public void EnsureLineCentral(int targetLineNumber)
        {
            if(targetLineNumber == -1)
                return;
            this.SelectedLine = targetLineNumber;
            //this.CentralLine = this.SelectedLine;
            ITextSnapshotLine ssLine = View.TextSnapshot.GetLineFromLineNumber(targetLineNumber);
            SnapshotSpan ssSpan = new SnapshotSpan(ssLine.Start, ssLine.End);

            this.Scroller.EnsureSpanVisible(ssSpan, EnsureSpanVisibleOptions.AlwaysCenter);
        }

        /// <summary>
        /// another version of getcentralline, for outsider use
        /// </summary>
        /// <param name="tv"></param>
        /// <returns></returns>
        public static ITextViewLine GetTopLine(ITextView tv,int centralLine, int offset)
        {
            if (tv == null)
            {
                return null;
            }
            ITextViewLineCollection tvlc;
            try
            {
                tvlc = tv.TextViewLines;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            if (tvlc == null) return null;                         //quite important, not applicable now
            int colSize = tvlc.Count;
            offset -= 1;                         //strange here
            if (offset > colSize / 2)
            {
                return null;
            }
            try
            {
                centralLine -= centralLine >= offset ? offset : centralLine;
                return tvlc.First(itv => TestMargin.GetViewLineNumber(itv) == centralLine);
                //double theight = tv.ViewportHeight / 2.0;
                //double lineHeight = tvlc.GetTextViewLineContainingYCoordinate(theight).Height;
                //return tvlc.GetTextViewLineContainingYCoordinate(theight - offset * lineHeight);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static ITextViewLine GetSpecViewLine(ITextView tv, int offset, TriBezierLines tbl)
        {
            if (tv == null)
            {
                return null;
            }
            ITextViewLineCollection tvlc;
            try
            {
                tvlc = tv.TextViewLines;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            if (tvlc == null) return null;                         //quite important, not applicable now
            int colSize = tvlc.Count;
            offset -= 1;                         //strange here, a bug
            double theight = tv.ViewportHeight / 2.0;
            try
            {
                double lineHeight = tvlc.GetTextViewLineContainingYCoordinate(theight).Height;
                double midoffset = lineHeight * offset * (TriBezierLines.Mid - tbl);
                return tvlc.GetTextViewLineContainingYCoordinate(theight - midoffset);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public int GetVisibleLineEdge(bool IsFirstLine) 
        {
            if (View == null)
            {
                return -1;
            }
            ITextViewLineCollection tvlc;
            try
            {
                tvlc = View.TextViewLines;
                return TestMargin.GetViewLineNumber(IsFirstLine?tvlc.FirstVisibleLine:tvlc.LastVisibleLine);
            }
            catch
            {
                return -1;
            }
        }

        public bool IsScrollerAtEdge(bool IsFirstLine) 
        {
            return this.GetVisibleLineEdge(IsFirstLine) == (IsFirstLine?0:(View.TextSnapshot.LineCount - 1));
        }
        #endregion
    }
}
