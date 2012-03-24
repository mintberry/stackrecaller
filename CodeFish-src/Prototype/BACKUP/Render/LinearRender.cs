using System;
using System.Drawing;

namespace Prototype
{
//	public struct LineLayout
//	{
//		public int y;
//		public int renderedSize;
//		public float fontSize;
//		public Boolean shown;
//		public Boolean inFocus;
//	}
	
	public class LinearRender : IRenderStrategy
	{
        public Layout GenerateLayout()
		{
            int minFontSize = Settings.Default.MinFontHeight;
            int maxFontSize = Settings.Default.Font.Height;

            Size size = View.Default.Size;

            DOIValue[] weights = Model.Default.DOIStrategy.Weights;

            FocusArea focus = Model.Default.Focus;

            Layout layout = new Layout(size, Model.Default.Length);

            int centerPoint = View.Default.Size.Height / 2;

            if (maxFontSize * focus.Center < centerPoint || size.Height > maxFontSize * weights.Length)
                centerPoint = (int)(maxFontSize * focus.Center); 
            else if (maxFontSize * (weights.Length - focus.Center - 1) < centerPoint)
                centerPoint = (int)(size.Height - maxFontSize - maxFontSize * (weights.Length - focus.Center - 1));

			for	(int i = 0; i < weights.Length; i++)
			{
                layout.lines[i].y = centerPoint + maxFontSize * (i - focus.Center);
                layout.lines[i].x = Model.Default.Indentation[i] * Settings.Default.WhitespaceWidth;
                layout.lines[i].shown = (layout.lines[i].y + maxFontSize < 0 || layout.lines[i].y > size.Height) ? false : true;
                layout.lines[i].height = layout.lines[i].shown ? maxFontSize : 0f;
                layout.lines[i].focus = focus.IsInside(i) ? 1f : 0f;
			}
				
			return layout;
		}
    }
}
