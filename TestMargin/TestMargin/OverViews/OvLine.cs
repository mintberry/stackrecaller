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
        //these variables are for global use, thus may be moved to new classes(collection)
        public static double lnInterval = 3.0;
        public static double lnStrokeTh = 1.6;
        public static double lnHoverStrokeTh = 2.0;

        
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

        Path myPath { get; set; }
        Canvas myCanvas { get; set; }
        OvCollection myParent { get; set; }

        public bool IsSeleted { get; set; }

        //to be modified

        //constructor
        public OvLine(Canvas canvas, ITextSnapshotLine itv, float bzCurvArea, OvCollection parent)
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

            myCanvas = canvas;
            myPath = new Path();
            myParent = parent;

            IsSeleted = false;
            

            this.myPath.MouseEnter += new System.Windows.Input.MouseEventHandler(myPath_MouseEnter);
            this.myPath.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(myPath_MouseLeftButtonDown);
            this.myPath.MouseLeave += new System.Windows.Input.MouseEventHandler(myPath_MouseLeave);

            
        }

        public void myPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SelectedChanged(false);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// this line is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myPath_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //this.OvLineSelected(this, null);
            this.myParent.OneSeleted(this.lnNumber);
            DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, lnStrokeTh, Brushes.DarkBlue);
            myPath.Margin = new Thickness(0.2);
            IsSeleted = true;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// this line is hovered on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myPath_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!IsSeleted)
                DrawSelfCmz(OvCollection.widperchar,OvCollection.divHeight,OvCollection.widRatio, lnHoverStrokeTh, Brushes.Gold);
            //throw new NotImplementedException();
            
        }

        /// <summary>
        /// draw oneself on canvas, initial draw, called by others
        /// </summary>
        /// <param name="c"></param>
        /// <param name="widperchar"></param>
        /// <param name="height"></param>
        /// <param name="widRate"></param>
        public void DrawSelf(Canvas c, float widperchar, float height, float widRate)
        {
            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(_bzCurvArea + lnTextStart * widperchar * widRate, (double)(lnNumber * height));
            myLineGeometry.EndPoint = new Point(_bzCurvArea + lnLength * widperchar * widRate, (double)(lnNumber * height));

            //myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = lnStrokeTh;
            myPath.Data = myLineGeometry;

            c.Children.Add(myPath);
        }


        public void DrawSelfCmz(float widperchar, float height, float widRate, double thickness, SolidColorBrush brushcolor)
        {
            //LineGeometry myLineGeometry = new LineGeometry();
            //myLineGeometry.StartPoint = new Point(_bzCurvArea + lnTextStart * widperchar * widRate, (double)(lnNumber * height));
            //myLineGeometry.EndPoint = new Point(_bzCurvArea + lnLength * widperchar * widRate, (double)(lnNumber * height));

            myPath.Stroke = brushcolor;
            myPath.StrokeThickness = thickness;
            //myPath.Data = myLineGeometry;

            //myCanvas.Children[this.lnNumber].InvalidateVisual();
            //myCanvas.Children[this.lnNumber] = myPath;
            
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

        public void SelectedChanged(bool IsChanged) 
        {
            if (IsChanged)
            {
                IsSeleted = false;
            }
            if (!IsSeleted)
                DrawSelfCmz(OvCollection.widperchar, OvCollection.divHeight, OvCollection.widRatio, lnStrokeTh, Brushes.Black);
            //throw new NotImplementedException();
        }
        
    }

}
