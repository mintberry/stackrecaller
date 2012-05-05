using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace TestMargin.Taggers
{
    internal sealed class OutlnTagger : ITagger<IOutliningRegionTag>
    {
        string startHide = "[";     //the characters that start the outlining region
        string endHide = "]";       //the characters that end the outlining region
        string ellipsis = "    ...";    //the characters that are displayed when the region is collapsed
        string hoverText = "check it out dumbass"; //the contents of the tooltip for the collapsed span
        ITextBuffer buffer;
        ITextSnapshot snapshot;
        List<Region> regions;
        ITextView view;

        TextInvisTagger _tit;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public OutlnTagger(ITextBuffer buffer, TextInvisTagger tit)
        {
            this.buffer = buffer;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();

            this._tit = tit;
            this.view = this._tit.View;
            //this.ReParse();
            this.buffer.Changed += BufferChanged;
            //this.outliningManager = ioMgr.GetOutliningManager(view);

            this._tit.OutlineRegionAggregated += new EventHandler<OutlineRegionAggregatedEventArgs>(_tit_OutlineRegionAggregated);
            
        }

        void _tit_OutlineRegionAggregated(object sender, OutlineRegionAggregatedEventArgs e)
        {
            List<Region> tobeOLed = new List<Region>(e.regions_aggregated);
            regions = tobeOLed;

            int central = e.Central;
            
            this.ReParse();

            _tit.EnsureCentral4Outline(central);
            
            _tit.IsOutlineFinished = true;    
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).
            if (e.After != buffer.CurrentSnapshot)
                return;
            //this.ReParse();//not sure this would work
        }

        void ReParse()
        {
            ITextSnapshot snap = this.snapshot;
            if (this.TagsChanged != null)
                this.TagsChanged(this, new SnapshotSpanEventArgs(
                    new SnapshotSpan(this.snapshot, Span.FromBounds(0, snapshot.Length))));
            
        }

        static bool TryGetLevel(string text, int startIndex, out int level)
        {
            level = -1;
            if (text.Length > startIndex + 3)
            {
                if (int.TryParse(text.Substring(startIndex + 1), out level))
                    return true;
            }

            return false;
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;
            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = this.snapshot;
            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, 
                spans[spans.Count - 1].End).TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);
            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;
            foreach (var region in currentRegions)
            {
                if (region.StartLine <= endLineNumber &&
                    region.EndLine >= startLineNumber)
                {
                    var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                    var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                    hoverText = snapshot.GetText(startLine.Start, endLine.End - startLine.Start);

                    //the region starts at the beginning of the "[", and goes until the *end* of the line that contains the "]".
                    yield return new TagSpan<IOutliningRegionTag>(
                        new SnapshotSpan(startLine.Start,
                        endLine.End),
                        new OutliningRegionTag(true, false, ellipsis, hoverText));
                }
            }
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);

            var endLine = snapshot.GetLineFromLineNumber(region.EndLine);

            return new SnapshotSpan(startLine.Start, endLine.End);
        }

    }

    class Region
    {
        public int StartLine { get; set; }
        public int EndLine { get; set; }

        public Region(int start, int end) 
        {
            StartLine = start;
            EndLine = end;
        }

        public int EliminatedLines() 
        {
            return (EndLine - StartLine);
        }
     
    }

}
