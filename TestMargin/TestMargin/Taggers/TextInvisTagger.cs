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
using TestMargin.OverViews;

namespace TestMargin.Taggers
{
    enum TextSyncType { 
        AllText,
        Hover,
        Central
    }

    class TextInvisTagger : ITagger<TextInvisTag>
    {
        IWpfTextView View { get; set; }                                      //iwpftextview
        ITextBuffer SourceBuffer { get; set; }
        //ITextSearchService TextSearchService { get; set; }
        //ITextStructureNavigator TextStructureNavigator { get; set; }
        NormalizedSnapshotSpanCollection WordSpans { get; set; }
        SnapshotSpan? CurrentWord { get; set; }
        SnapshotPoint RequestedPoint { get; set; }

        EditorActor Actor { get; set; }
        emuParser Parser { get; set; }

        //OvCollection overView { get; set; }

        object updateLock = new object();

        IClassificationTypeRegistryService _ctrs { set; get; }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;       //

        public event EventHandler<TextViewLayoutChangedEventArgs> ScrollNumberFixed;

        public TextInvisTagger(ITextView view, ITextBuffer sourceBuffer/*, ITextSearchService textSearchService,
            ITextStructureNavigator textStructureNavigator*/, IClassificationTypeRegistryService ctrs)
        {
            this.View = view as IWpfTextView;
            this.SourceBuffer = sourceBuffer;
            //this.TextSearchService = textSearchService;
            //this.TextStructureNavigator = textStructureNavigator;
            this.WordSpans = new NormalizedSnapshotSpanCollection();
            this.CurrentWord = null;
            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
            this.View.MouseHover += new EventHandler<MouseHoverEventArgs>(View_MouseHover);
            this.ScrollNumberFixed += new EventHandler<TextViewLayoutChangedEventArgs>(TextInvisTagger_ScrollNumberFixed);


            this.Actor = new EditorActor(View);
            this.Parser = new emuParser(View.TextSnapshot, Actor);

            //this.overView = ovc;
            //ovc.OvLineSelected += new EventHandler<OvCollectionEventArgs>(ovc_OvLineSelected);

            //not known whether here is okay
            Parser.BuildTrees();

            this._ctrs = ctrs;
        }


        void ovc_OvLineSelected(object sender, OvCollectionEventArgs e)
        {
            //throw new NotImplementedException();
            int lineSel = e.LineSelected;
            int lastSel = e.LastLineSelected;

            int diff = lineSel - lastSel;
            this.Actor.ScrollLines(lineSel, diff);
            //trigger a event or call ViewLayoutChanged directly
            //this ia a brute force bug fix
            this.ScrollNumberFixed(this, null);
        }

        void TextInvisTagger_ScrollNumberFixed(object sender, TextViewLayoutChangedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e == null)
            {
                int centralLine = Actor.GetCentralLine();
                if (centralLine == -1) return;
                Parser.GenDispType(centralLine);

                SyncText(TextSyncType.AllText);
            }
        }

        void View_MouseHover(object sender, MouseHoverEventArgs e)
        {
            //throw new NotImplementedException();
            SnapshotPoint? sspq = e.TextPosition.GetPoint(View.TextSnapshot,PositionAffinity.Predecessor);
            
            if(sspq.HasValue)
            {
                SnapshotPoint ssp = new SnapshotPoint(SourceBuffer.CurrentSnapshot, e.Position);
                ITextSnapshotLine hoverLine = ssp.GetContainingLine();
                Actor.HoverLine = hoverLine.LineNumber;

                if(Actor.GetCentralLine() != Actor.HoverLine)
                {
                    SyncText(TextSyncType.Hover);
                }
                
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            CaretPosition cp = e.NewPosition;
            SnapshotPoint? ssp = cp.Point.GetPoint(SourceBuffer, cp.Affinity);
            if (!ssp.HasValue) return;
            RequestedPoint = ssp.Value;

            ITextSnapshotLine selectedLine = RequestedPoint.GetContainingLine();
            SnapshotSpan span = selectedLine.Extent;                          // the line containing the position
            NormalizedSnapshotSpanCollection col = new NormalizedSnapshotSpanCollection(span);

            CurrentWord = span;
            WordSpans = col;


            int selectedLineNumber = selectedLine.LineNumber;
            int diff = selectedLineNumber - Actor.CentralLine;

            this.Actor.ScrollLines(selectedLineNumber, diff);
            //trigger a event or call ViewLayoutChanged directly
            //this ia a brute force bug fix
            this.ScrollNumberFixed(this, null);

            //ensure that this called ahead of the overview
            //Actor.ValidateScroll();
            //SyncText(TextSyncType.Central);           //not trigger the event, just change vertical layout
        }
        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.VerticalTranslation == true)                //scroll vertically
            {
                int centralLine = Actor.GetCentralLine();
                //System.Diagnostics.Trace.WriteLine("%%%                 CENTRAL: " + Actor.CentralLine);
                if (centralLine == -1) return;
                Parser.GenDispType(centralLine);

                SyncText(TextSyncType.AllText);
            }
        }

        private void SyncText(TextSyncType synctype)
        {
            lock (updateLock)
            {
                if (TagsChanged != null)
                {
                    //raise an event, but why the span is whole?
                    TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot,
                                 0, SourceBuffer.CurrentSnapshot.Length)));
                }
            }
        }



        #region ITagger<TextInvisTag> Members

        IEnumerable<ITagSpan<TextInvisTag>> ITagger<TextInvisTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //the spans shall be used
            if (CurrentWord.HasValue)
            {
                //if (spans.OverlapsWith(new NormalizedSnapshotSpanCollection(CurrentWord.Value)))
                //    yield return new TagSpan<TextInvisTag>(CurrentWord.Value, new TextInvisTag(_ctrs.GetClassificationType("invisclass.careton")));
                if (Actor.HoverLine != Actor.CentralLine)
                    yield return GetTagSpanFromLineNumber(Actor.HoverLine, "invisclass.lower.hover", spans);
                yield return GetTagSpanFromLineNumber(Actor.CentralLine, "invisclass.central", spans);

                foreach (LineEntity le in Parser.consLineEntity)
                {
                    //hard code here
                    if (le != null && le.DisT == DisplayType.Dismiss)
                        yield return GetTagSpanFromLineNumber(le.LineNumber, "invisclass.invis", spans);
                    //
                }
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
            if (lineNumber == -1)
            {
                return null;
            }
            return View.TextSnapshot.GetLineFromLineNumber(lineNumber).Extent;
        }

        /// <summary>
        /// create new tagspan from line number
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        ITagSpan<TextInvisTag> GetTagSpanFromLineNumber(int lineNumber, string classification, 
            NormalizedSnapshotSpanCollection spans)
        {
            if (lineNumber == -1)
            {
                return null;
            }
            SnapshotSpan? sspanq = emuParser.GetTextSpanFromLineNumber(View, lineNumber);
            if (sspanq.HasValue /*&& spans.OverlapsWith(new NormalizedSnapshotSpanCollection(sspanq.Value))*/)
            {
                SnapshotSpan sspan = sspanq.Value;
                //GetSpanFromLineNumber(lineNumber).Value; use new emuParser span
                return new TagSpan<TextInvisTag>(sspan, new TextInvisTag(_ctrs.GetClassificationType(classification)));
            }
            else
            {
                return null;
            }
        }

        public void Scroll4OverView(int lnSel, int lastlnSel) 
        {
            //throw new NotImplementedException();
            int lineSel = lnSel;
            int lastSel = lastlnSel;

            int diff = lineSel - lastSel;
            this.Actor.ScrollLines(lineSel, diff);
            //trigger a event or call ViewLayoutChanged directly
            //this ia a brute force bug fix
            this.ScrollNumberFixed(this, null);
        }

        public int Scroll4OvLine()
        {
            return Actor.GetCentralLine();
        }

        public int GetCentralLine4Ov() 
        {
            return this.Actor.CentralLine;
        }

        #endregion
    }
}
