using System;
using System.Drawing;
using System.Collections.Generic;

namespace Prototype
{

	public class FisheyeRender : IRenderStrategy
	{
        private const float TINY_FONT_SIZE = 3f;

		struct IndexWeight
		{
			public IndexWeight(int index, float weight)
			{
				this.index = index;
				this.weight = weight;
			}
				
			public int index;
			public float weight;
		}
		
		#region Comparers
		public class IndexComparer : IComparer<IndexWeight>
		{
			int IComparer<IndexWeight>.Compare( IndexWeight x, IndexWeight y )
			{
				return (x.index < y.index) ? -1 : 1;
			}
		}
		
		public class WeightComparer : IComparer<IndexWeight>
		{
			// reverse sort as we need a list sorted by descending weight
			int IComparer<IndexWeight>.Compare( IndexWeight x, IndexWeight y )
			{
				return (x.weight > y.weight) ? -1 : 1;
			}

		}
		#endregion
		
		// Returns list of index/weight pairs for items in range start->end
        private List<IndexWeight> SortWeights(DOIValue[] weights, int start, int end)
		{
			List<IndexWeight> list = new List<IndexWeight>();
			for (int i = start; i < end; i++)
				list.Add(new IndexWeight(i, weights[i].weight));
			
			// Sort by order of descending weight
			list.Sort(new WeightComparer());
			
			return list;		
		}
		
		// Pick highest weighted items while sum of heights is less than the height of the area
		// and returns them sorted by original index(line number).
        private int FillArea(int height, ref List<IndexWeight> list, float maxFontSize, float minFontSize)
		{
			List<IndexWeight> shownList = new List<IndexWeight>();
			
			int totalHeight = 0;
            
			foreach (IndexWeight line in list)
			{
                int lineHeight = (int)Math.Max(maxFontSize * line.weight, minFontSize);

                if (totalHeight + lineHeight >= height)
                    break; 
                
                totalHeight += lineHeight;
                shownList.Add(line);
			}
			
			shownList.Sort(new IndexComparer());
			list = shownList;
			
			return totalHeight;
		}

        public Layout GenerateLayout()
        {
            int minFontSize = Settings.Default.MinFontHeight;
            int maxFontSize = Settings.Default.Font.Height;

            Size size = View.Default.Size;

            DOIValue[] weights = Model.Default.DOIStrategy.Weights;

            FocusArea focus = Model.Default.Focus;

            Layout layout = new Layout(size, Model.Default.Length);

            int fontHeight = (int)maxFontSize;

            int focusHeight = fontHeight * Model.Default.Focus.Lines;
            int contextHeight = size.Height - focusHeight;

            int contextLines = weights.Length - Model.Default.Focus.Lines;
            int topLines = focus.Start;
            int bottomLines = weights.Length - Model.Default.Focus.End - 1;

            int topHeight = (contextHeight * topLines) / contextLines;
            int bottomHeight = (contextHeight * bottomLines) / contextLines;

            // Layout focus area
            for (int i = Model.Default.Focus.Start; i <= Model.Default.Focus.End; i++)
            {
                layout.lines[i].focus = 1f;
                layout.lines[i].shown = true;
                layout.lines[i].height = fontHeight;
                layout.lines[i].y = topHeight + fontHeight * (i - Model.Default.Focus.Start);
            }

            for (int i = 0; i < layout.lines.Length; i++)
            {
                layout.lines[i].x = 5 + Model.Default.Indentation[i] * Settings.Default.WhitespaceWidth;
                layout.lines[i].doi = weights[i];
            }

            // Layout top context
            LayoutArea(weights, maxFontSize, minFontSize, ref layout, 0, topLines, topHeight, 0, false);
            LayoutArea(weights, maxFontSize, minFontSize, ref layout, weights.Length - bottomLines, weights.Length, bottomHeight, size.Height - bottomHeight - 1, true);

            AddMethodWeights(ref weights, layout);
            
            ClearLayout(ref layout, 0, topLines);
            ClearLayout(ref layout, weights.Length - bottomLines, weights.Length);

            LayoutArea(weights, maxFontSize, minFontSize, ref layout, 0, topLines, topHeight, 0, false);
            LayoutArea(weights, maxFontSize, minFontSize, ref layout, weights.Length - bottomLines, weights.Length, bottomHeight, size.Height - bottomHeight - 1, true);

            FixLayout(ref layout);

            return layout;
        }

        private void FixLayout(ref Layout layout)
        {
            for (int i = 0; i < layout.lines.Length; i++)
            {
                if (!layout.lines[i].shown)
                {
                    layout.lines[i].height = 0f;
                }
            }
        }

        private void AddMethodWeights(ref DOIValue[] weights, Layout layout)
        {
            for (int i = 0; i < layout.lines.Length; i++)
            {
                if (layout.lines[i].shown)
                {
                    BlockInfo bi = Model.Default.BlockInfoMapping[i];

                    if (bi != null)
                        weights[bi.StartLine].weight = 1f;
                }
            }
        }

        private void ClearLayout(ref Layout layout, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                layout.lines[i].shown = false;
            }
        }

        private int LayoutArea(DOIValue[] weights, float maxFontSize, float minFontSize, ref Layout layout, int start, int end, int height, int y, bool alignToBottom)
        {
            List<IndexWeight> sortedList = SortWeights(weights, start, end);
            int itemsHeight = FillArea(height, ref sortedList, maxFontSize, minFontSize);
			
            if(alignToBottom)
             y = y + height - itemsHeight;
            
            foreach (IndexWeight item in sortedList)
            {
                layout.lines[item.index].focus = 0f;
                layout.lines[item.index].shown = true;
                layout.lines[item.index].height = (int)Math.Max(maxFontSize * item.weight, minFontSize);
                layout.lines[item.index].y = y;
                y += (int)layout.lines[item.index].height;
            }
            
            return y;
        }
    }
}
