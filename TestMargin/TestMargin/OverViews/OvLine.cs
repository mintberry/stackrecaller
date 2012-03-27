using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;


namespace TestMargin.OverViews
{

    class OvLine
    {
        public float _bzCurvArea;                    //area before overview

        public int   lnNumber;                      //starts from 0
        public float lnStart;
        public float lnTextStart;
        public float lnEnd;
        //public float lnElev;
        public float lnHeight;
        public float lnLength;
        public Color lnColor;
        public bool  lnFocus;
        //to be modified

        //constructor
        public OvLine(ITextSnapshotLine itv, float bzCurvArea)
        {
            _bzCurvArea = bzCurvArea;

            lnNumber = itv.LineNumber;
            lnStart = 0.0f;
            lnTextStart = 0.0f;
            lnEnd = (float)itv.Length;
            lnHeight = 1.0f;
            lnLength = itv.Length;
            lnColor = new System.Windows.Media.Color();
            lnFocus = false;
        }

        //draw self on canvas
        public void DrawSelf(Canvas c, float widRate, float height)
        {
            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(_bzCurvArea + lnStart * widRate, (double)(lnNumber));
            myLineGeometry.EndPoint = new Point(_bzCurvArea + lnEnd * widRate, (double)(lnNumber));

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = myLineGeometry;

            c.Children.Add(myPath);
        }

        
    }
}
