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
using TestMargin.Utils;

namespace TestMargin.Taggers
{
    class TextInvisTagger : ITagger<TextInvisTag>
    {
        ITextView View { get; set; }                                      //iwpftextview
        ITextBuffer SourceBuffer { get; set; }
        //ITextSearchService TextSearchService { get; set; }
        //ITextStructureNavigator TextStructureNavigator { get; set; }
        NormalizedSnapshotSpanCollection WordSpans { get; set; }
        SnapshotSpan? CurrentWord { get; set; }
        SnapshotPoint RequestedPoint { get; set; }

        EditorActor Actor { get; set; }
        emuParser Parser { get; set; }
         
        object updateLock = new object();

        IClassificationTypeRegistryService _ctrs { set; get; }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;       //

        public TextInvisTagger(ITextView view, ITextBuffer sourceBuffer/*, ITextSearchService textSearchService,
            ITextStructureNavigator textStructureNavigator*/, IClassificationTypeRegistryService ctrs)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            //this.TextSearchService = textSearchService;
            //this.TextStructureNavigator = textStructureNavigator;
            this.WordSpans = new NormalizedSnapshotSpanCollection();
            this.CurrentWord = null;
            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;

            Actor = new EditorActor(View);
            Parser = new emuParser(View.TextSnapshot, Actor);

            //not known whether here is okay
            Parser.BuildTrees();

            this._ctrs = ctrs;
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e) 
        {
            CaretPosition cp = e.NewPosition;
            SnapshotPoint? ssp = cp.Point.GetPoint(SourceBuffer, cp.Affinity);
            if (!ssp.HasValue) return;
            RequestedPoint = ssp.Value;

            SnapshotSpan span = RequestedPoint.GetContainingLine().Extent;                          // the line containing the position
            NormalizedSnapshotSpanCollection col = new NormalizedSnapshotSpanCollection(span);

            CurrentWord = span;
            WordSpans = col;

            SyncText();
        }
        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e) 
        {
            if(e.VerticalTranslation == true)                //scroll vertically
            {
                int centralLine = Actor.GetCentralLine();
                SyncText();
            }
        }

        private void SyncText() 
        {
            lock (updateLock)
            {
                if (TagsChanged != null)
                {
                    //raise an event, but why the span is whole?
                    TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot, 0, SourceBuffer.CurrentSnapshot.Length)));
                }
            }
        }



        #region ITagger<TextInvisTag> Members

        IEnumerable<ITagSpan<TextInvisTag>> ITagger<TextInvisTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (CurrentWord.HasValue)
            {
                if (spans.OverlapsWith(new NormalizedSnapshotSpanCollection(CurrentWord.Value)))
                    yield return new TagSpan<TextInvisTag>(CurrentWord.Value, new TextInvisTag(_ctrs.GetClassificationType("invisclass.careton")));
                yield return GetTagSpanFromLineNumber(Actor.CentralLine);
            }
            //throw new NotImplementedException();
            //foreach (SnapshotSpan span in spans)
            //{
            //    yield return new TagSpan<TextInvisTag>(span, new TextInvisTag());
            //}
        }
        #endregion

        #region Helpers
        /// <summary>
        /// get textsnapshotspan from line number
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        SnapshotSpan? GetSpanFromLineNumber(int lineNumber)
        {
            return View.TextSnapshot.GetLineFromLineNumber(lineNumber).Extent;
        }

        /// <summary>
        /// create new tagspan from line number
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        ITagSpan<TextInvisTag> GetTagSpanFromLineNumber(int lineNumber)
        {
            SnapshotSpan sspan = GetSpanFromLineNumber(lineNumber).Value;
            return new TagSpan<TextInvisTag>(sspan, new TextInvisTag(_ctrs.GetClassificationType("invisclass.careton")));
        }


        #endregion
    }
}
