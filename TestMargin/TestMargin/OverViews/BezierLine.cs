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
using Microsoft.VisualStudio.Text.Formatting;

namespace TestMargin.OverViews
{
    class BezierLine
    {
        public OvLine RightOvLine { get; set; }
        public ITextViewLine LeftTvLine { get; set; }
        //Point LeftPoint { get; set; }
        //Point RightPoint { get; set; }
        //Point CtrlPoint { get; set; }
        public double height { get; set; }  //there should be an offset between the editor geo and ov geo
                                     //

        public double leftPointY = -1.0;    //this should be bottom of the textviewline

        TriBezierLines tblType { get; set; }

        Path myPath { get; set; }

        public BezierLine(TriBezierLines tbl) 
        {
            myPath = new Path();
            tblType = tbl;
        }

        public BezierLine(ITextViewLine itvLine, OvLine rightln) 
        {
            RightOvLine = rightln;
            LeftTvLine = itvLine;
            myPath = new Path();
        }

        public void DrawSelf(Canvas c, bool IsFirstdraw = false)
        {
            
            double rightY = RightOvLine.lnNumber *  OvCollection.divHeight;
            //leftPointY = LeftTvLine.Top;
            //switch (this.tblType)
            //{
            //    case TriBezierLines.Bot:
            //        leftPointY = LeftTvLine.Bottom;
            //        break;
            //    case TriBezierLines.Mid:
            //        leftPointY = LeftTvLine.Bottom - LeftTvLine.Height / 2.0;
            //        break;
            //    case TriBezierLines.Top:
            //        leftPointY = LeftTvLine.Top;
            //        break;
            //    default:
            //        break;
            //}

            Point LeftPoint = new Point(0.0, leftPointY);
            Point RightPoint = new Point(RightOvLine._bzCurvArea, rightY);
            
            Point CtrlPoint1 = new Point(RightOvLine._bzCurvArea / 2.0, leftPointY);
            Point CtrlPoint2 = new Point(RightOvLine._bzCurvArea / 2.0, rightY);

            PathFigure pf = new PathFigure();

            pf.StartPoint = LeftPoint;

            BezierSegment bs1 = new BezierSegment(CtrlPoint1, CtrlPoint2, RightPoint, true);
            //BezierSegment bs2 = new BezierSegment(MidPoint, CtrlPoint2, RightPoint, true);
            pf.Segments.Add(bs1);
            //pf.Segments.Add(bs2);
            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);


            //myPath = new Path();
            if (this.tblType == TriBezierLines.Hover)
            {
                myPath.Stroke = Brushes.LightGray;
            }
            else
                myPath.Stroke = Brushes.CornflowerBlue;
            myPath.StrokeThickness = OvLine.lnStrokeTh;
            myPath.Data = pg;

            if (/*IsFirstdraw && */!c.Children.Contains(myPath))
                c.Children.Add(myPath);
        }


    }
}
