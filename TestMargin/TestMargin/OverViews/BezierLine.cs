﻿using System;
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
        double offset { get; set; }  //there should be an offset between the editor geo and ov geo

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
            double rightY = RightOvLine.lnNumber * OvCollection.divHeight;
            double leftY = LeftTvLine.Top;
            switch (this.tblType)
            {
                case TriBezierLines.Bot:
                    leftY = LeftTvLine.Bottom;
                    break;
                case TriBezierLines.Mid:
                    leftY = (LeftTvLine.Bottom + LeftTvLine.Top) / 2.0;
                    break;
                case TriBezierLines.Top:
                    leftY = LeftTvLine.Top;
                    break;
                default:
                    break;
            }

            Point LeftPoint = new Point(0.0, LeftTvLine.Top);
            Point RightPoint = new Point(RightOvLine._bzCurvArea, rightY);
            Point MidPoint = new Point(RightOvLine._bzCurvArea / 2.0, (LeftTvLine.Top + rightY) / 2.0);
            Point CtrlPoint1 = new Point(RightOvLine._bzCurvArea / 2.0, LeftTvLine.Top);
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
            myPath.Stroke = Brushes.LightGray;
            myPath.StrokeThickness = OvLine.lnStrokeTh;
            myPath.Data = pg;

            if (IsFirstdraw)
                c.Children.Add(myPath);
            
        }


    }
}
