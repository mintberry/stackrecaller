using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace TestMargin.Utils
{
    /// <summary>
    /// control the editor scrolling actions
    /// </summary>
    class EditorActor
    {
        ITextView View { get; set; }                                      //iwpftextview
        public int CentralLine { get; set; }
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
            return CentralLine = TestMargin.GetViewLineNumber(tvlc[colSize / 2]);
        }

        bool IsEdge(int toporbot)
        {
            ITextViewLineCollection tvlc = View.TextViewLines;
            if (tvlc == null) return true;                         //quite important

            int colSize = tvlc.Count;
            return TestMargin.GetViewLineNumber(tvlc.FirstVisibleLine) == toporbot;
        }

        public void ValidateScroll() 
        {
            this.Scroller.ScrollViewportVerticallyByPixels(0.1);
            this.Scroller.ScrollViewportVerticallyByPixels(-0.1);
        }
        #endregion
    }
}
