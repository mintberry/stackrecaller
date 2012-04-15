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
        public IViewScroller Scroller { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public EditorActor(ITextView itv) 
        {
            this.View = itv;
            Scroller = View.ViewScroller;
            GetCentralLine();
        }



        #region Helpers
        /// <summary>
        /// </summary>
        /// <returns>central viewline number</returns>
        public int GetCentralLine() 
        {
            ITextViewLineCollection tvlc = View.TextViewLines;
            if (tvlc == null) return 0;                         //quite important
            int colSize = tvlc.Count;
            return CentralLine = TestMargin.GetViewLineNumber(tvlc[colSize / 2]);
        }
        #endregion
    }
}
