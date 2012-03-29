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

namespace TestMargin.Taggers
{
    class TextInvisTagger : ITagger<TextInvisTag>
    {
        ITextView View { get; set; }                                      //iwpftextview
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }
        NormalizedSnapshotSpanCollection WordSpans { get; set; }
        SnapshotSpan? CurrentWord { get; set; }
        SnapshotPoint RequestedPoint { get; set; }
        object updateLock = new object();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;       //

        public TextInvisTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService textSearchService,
            ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;
            this.WordSpans = new NormalizedSnapshotSpanCollection();
            this.CurrentWord = null;
            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) 
        {
            CaretPosition cp = e.NewPosition;
            SnapshotPoint? ssp = cp.Point.GetPoint(SourceBuffer, cp.Affinity);
            if (!ssp.HasValue) return;
            RequestedPoint = ssp.Value;

            SnapshotSpan span = RequestedPoint.GetContainingLine().Extent;                          // the line containing the position
            NormalizedSnapshotSpanCollection col = new NormalizedSnapshotSpanCollection(span);


        }
        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) 
        {
            if(e.VerticalTranslation == true)                //scroll vertically
            {

            }
        }

        private void SyncText() 
        {
            lock (updateLock)
            {

            }
        }



        #region ITagger<TextInvisTag> Members

        IEnumerable<ITagSpan<TextInvisTag>> ITagger<TextInvisTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            throw new NotImplementedException();
        }

        event EventHandler<SnapshotSpanEventArgs> ITagger<TextInvisTag>.TagsChanged
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        #endregion
    }
}
