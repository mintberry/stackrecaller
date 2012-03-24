using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace Prototype
{
    public class Layout
    {
        public Layout(Size size, int lines)
        {
            this.size = size;
            this.lines = new LineLayout[lines];
        }

        public static Layout Blend(Layout l1, Layout l2, float a)
        {
            Debug.Assert(l1.lines.Length == l2.lines.Length);

            Layout layout = new Layout(l1.size, l1.lines.Length);

            LineLayout[] lines = layout.lines;

            float y = 0;

            int lastshown1 = -1;
            int lastshown2 = -1;

            for (int i = 0; i < l1.lines.Length; i++)
            {
                lines[i] = Blend(l1.lines[i], l2.lines[i], a);
                
                if (layout.lines[i].shown)
                {
                    if (l1.lines[i].shown)
                    {
                        if (lastshown1 == -1)
                        {
                            y += (int)(l1.lines[i].y * (1f - a));
                            lastshown1 = i;
                        }
                        else
                        {
                            float space = (l1.lines[i].y - l1.lines[lastshown1].y - l1.lines[lastshown1].height) * (1f - a);
                            y += space;
                            lastshown1 = i;
                        }
                    }

                    if (l2.lines[i].shown)
                    {
                        if (lastshown2 == -1)
                        {
                            y += (int)(l2.lines[i].y * a);
                            lastshown2 = i;
                        }
                        else
                        {
                            float space = (l2.lines[i].y - l2.lines[lastshown2].y - l2.lines[lastshown2].height) * a;
                            y += space;
                            lastshown2 = i;
                        }
                    }

                    lines[i].y = y;
                    y += lines[i].height;
                }
            }

            return layout;
        }

        public static LineLayout Blend(LineLayout ll1, LineLayout ll2, float a)
        {
            LineLayout result = new LineLayout();

            result.x = Blend(ll1.x, ll2.x, a);
            result.y = Blend(ll1.y, ll2.y, a);
            result.height = Blend(ll1.height, ll2.height, a);
            result.shown = ll1.shown | ll2.shown;
            result.focus = Blend(ll1.focus, ll2.focus, a);
            result.doi.semantic = Blend(ll1.doi.semantic, ll2.doi.semantic, a);
            result.doi.weight = Blend(ll1.doi.weight, ll2.doi.weight, a);
            result.doi.importance = Blend(ll1.doi.importance, ll2.doi.importance, a);

            return result;
        }

        private static float Blend(float v1, float v2, float a)
        {
            return v1 * (1f - a) + v2 * a;
        }

        private static int Blend(int v1, int v2, float a)
        {
            return  (int)(v1 * (1f - a) + v2 * a + 0.5f);
        }

        public Size size;
        public LineLayout[] lines;
    }

    public struct LineLayout
    {
        public float x;
        public float y;
        public float height;
        public Boolean shown;
        public float focus;
        public DOIValue doi;
    }
}
