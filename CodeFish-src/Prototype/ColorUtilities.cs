using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Prototype
{
    class ColorUtilities
    {
        public static float Interpolate(float v1, float v2, float a)
        {
            return v1 * (1f - a) + v2 * a;
        }

        public static int Interpolate(int v1, int v2, float a)
        {
            return (int)Interpolate((float)v1, (float)v2, a);
        }

        public static float Brighten(float a, float brightness)
        {
            return 255 * brightness + a * (1f - brightness);
        }

        public static Color ToOverviewColor(Color color)
        {
            float saturation = 1.0f;
            float brightness = 0.6f;

            float greyLevel = color.R * 0.297f + color.G * 0.585f + color.B * 0.142f;
            return Color.FromArgb((byte)Brighten(Interpolate(greyLevel, color.R, saturation), brightness),
                                  (byte)Brighten(Interpolate(greyLevel, color.G, saturation), brightness),
                                  (byte)Brighten(Interpolate(greyLevel, color.B, saturation), brightness));
        }

        public static Color Interpolate(Color c1, Color c2, float a)
        {
            return Color.FromArgb(
                (byte)Interpolate(c1.R, c2.R, a),
                (byte)Interpolate(c1.G, c2.G, a),
                (byte)Interpolate(c1.B, c2.B, a));
        }
    }
}