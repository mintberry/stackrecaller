using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
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
            lnTextStart = (float)Find1stChar(itv);
            //if (lnNumber == 65) System.Diagnostics.Trace.WriteLine("%%%                 REGEX: " + lnTextStart ); 
            lnEnd = (float)itv.Length;
            lnHeight = 1.0f;
            lnLength = itv.Length;
            lnColor = new System.Windows.Media.Color();                    //get the color of the textview
            lnFocus = false;
        }

        //draw self on canvas
        public void DrawSelf(Canvas c, float widperchar, float height, float widRate)
        {
            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(_bzCurvArea + lnTextStart * widRate, (double)(lnNumber * 3));
            myLineGeometry.EndPoint = new Point(_bzCurvArea + lnLength * widperchar * widRate, (double)(lnNumber * 3));

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = myLineGeometry;

            c.Children.Add(myPath);
        }

        private int Find1stChar(ITextSnapshotLine tsl) 
        {
            string s = tsl.GetText();
            int i1stChar = Regex.Match(s, @"\S").Index;
            int i1stTab = Regex.Match(s, @"\t").Index;
            if(i1stTab < i1stChar)
            {
                int iTabCount = Regex.Matches(s, @"\t").Count;
                if (iTabCount <= i1stChar)
                {
                    i1stChar = iTabCount * 4 + i1stChar - iTabCount;
                }
            }
            
            return i1stChar;
        }

        
    }
}
