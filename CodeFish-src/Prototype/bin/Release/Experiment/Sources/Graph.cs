using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TradeOff
{
    public class Graph : Panel
    {
        #region Fields
        // Control propeties
        private ArrayList _DataSources; // List of DataChannels
        private double _HighestValue = Double.NegativeInfinity;
        private double _LowestValue = Double.PositiveInfinity;
        private double _OffsetX;
        private int _WorkingHeight;

        // Local global values
        private int _TimeSpan;
        private int _TopBottomMargin;
        private int _GridLines;
        private int _MarkerSize;
        private int _GraphThickness;
        private bool _MouseDown;
        #endregion

        #region Get / Set
        public int GridLines
        {
            get { return _GridLines; }
            set { _GridLines = value; }
        }

        public int TopBottomMargin
        {
            get { return _TopBottomMargin; }
            set
            {
                _TopBottomMargin = value;
                _WorkingHeight = Height - (2 * _TopBottomMargin);
            }
        }

        public int TimeSpan
        {
            get { return _TimeSpan; }
            set
            {
                _TimeSpan = value;
                _OffsetX = this.Width / (double)_TimeSpan;
            }
        }

        public int MarkerSize
        {
            get { return _MarkerSize; }
            set
            {
                if (value % 2 == 1)
                    _MarkerSize = value + 1;
                else
                    _MarkerSize = value;
            }
        }

        public int GraphThickness
        {
            get { return _GraphThickness; }
            set { _GraphThickness = value; }
        }
        #endregion

        #region Constructor

        public Graph()
        {
            // Default values
            TimeSpan = 10;
            TopBottomMargin = 5;
            GridLines = 4;
            GraphThickness = 2;
            MarkerSize = 6;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            _DataSources = new ArrayList();
        }
        #endregion

        #region Data channel creation
        // Creating a graph for the graphing module and returning the index for the graph

        public int CreateDataChannel(Color c, string n)
        {
            int index = _DataSources.Add(new DataChannel(c, n));
            return index;
        }


        public int CreateDataChannel(Color c)
        {
            int index = _DataSources.Add(new DataChannel(c));
            return index;
        }
        #endregion

        #region Data insertion methods

        // Inserting an event for a given graph
        public void InsertEvent(int index, string e)
        {
            ((DataChannel)_DataSources[index]).AddEvent(e);
        }


        // Adding a value to a given graph. (Moving time forward)
        public void Insert(int index, double data)
        {
            DataChannel dc = (DataChannel)_DataSources[index];
            dc.Enqueue(data);
            if (dc.Display)
            {
                _HighestValue = Math.Max(_HighestValue, data);
                _LowestValue = Math.Min(_LowestValue, data);
            }
            if (dc.Count >= _TimeSpan)
            {
                double deq = (double)dc.Dequeue();

                if (dc.Display)
                {
                    if (deq == _LowestValue || deq == _HighestValue)
                        FindMaxMinValues();
                }
            }
            this.Invalidate();
        }


        // Note: d[0] = index of data channel
        //       d[1] = value to be inserted
        public void Inserts(ArrayList data)
        {
            bool MaxMinChanged = false;
            foreach (double[] d in data) //d[0] = index of data channel, d[1] = value to be inserted
            {
                DataChannel dc = (DataChannel)_DataSources[(int)d[0]];
                dc.Enqueue(d[1]);
                if (dc.Display)
                {
                    _HighestValue = Math.Max(_HighestValue, d[1]);
                    _LowestValue = Math.Min(_LowestValue, d[1]);
                }
                if (dc.Count >= _TimeSpan)
                {
                    double deq = (double)dc.Dequeue();

                    if (dc.Display)
                    {
                        if (deq == _LowestValue || deq == _HighestValue)
                            MaxMinChanged = true;
                    }
                }
            }
            if (MaxMinChanged)
                FindMaxMinValues();

            this.Invalidate();
        }
        #endregion

        #region Display status methods

        // Hiding or showing a given graph
        public void Display(int index, bool display)
        {
            ((DataChannel)_DataSources[index]).Display = display;
            FindMaxMinValues();
            this.Invalidate();
        }
        #endregion

        #region Utility methods
        // Updating the control max and min values.
        private void FindMaxMinValues()
        {
            _HighestValue = Double.NegativeInfinity;
            _LowestValue = Double.PositiveInfinity;
            foreach (DataChannel dc in _DataSources)
            {
                if (dc.Display)
                {
                    _HighestValue = Math.Max(_HighestValue, dc.Max);
                    _LowestValue = Math.Min(_LowestValue, dc.Min);
                }
            }

            // Just making sure that the y-axis doesn't fuck up
            if (Double.IsInfinity(_LowestValue))
                _LowestValue = 0;
            if (Double.IsInfinity(_HighestValue))
                _HighestValue = 100;

        }


        private int CalculateY(double data)
        {
            return (_WorkingHeight - (int)Math.Round(((Math.Abs(data - _LowestValue)) / (Math.Abs(_HighestValue - _LowestValue))) * _WorkingHeight)) + _TopBottomMargin;
        }
        #endregion


        // Painting the control
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int y;
            // Drawing gridlines
            for (int i = 0; i < GridLines + 1; i++)
            {
                y = (i * (int)Math.Round(_WorkingHeight / (double)GridLines)) + _TopBottomMargin;
                g.DrawLine(new Pen(Brushes.LightGray, 1), 0, y, Width, y);
                g.DrawString(String.Format("{0:0,0}", _HighestValue - (i * ((_HighestValue - _LowestValue) / (double)GridLines))), new Font("Arial", 6), Brushes.Gray, 0, y);
            }

            // Drawing graphs

            // This currently takes O(n*m), where n = data count in given graph, and m = number of graphs.
            // If you make sure that every graph has the same data count, then it can be implemented in O(n) by only itterating over the
            // the data count and accessing each graphs data at every data count.
            // Dynamic checking for this constraint could be inplemented easy.
            foreach (DataChannel dc in _DataSources)
            {
                if (dc.Count > 1 && dc.Display)
                {
                    IEnumerator ie = dc.GetEnumerator();
                    ie.MoveNext();
                    double prevData = (double)ie.Current;

                    for (int i = 1; ie.MoveNext(); i++)
                    {
                        double currData = (double)ie.Current;
                        int prevY = CalculateY(prevData);
                        int currY = CalculateY(currData);
                        g.DrawLine(new Pen(dc.Color, GraphThickness), (int)Math.Round((i * _OffsetX)), prevY, (int)Math.Round((i + 1) * _OffsetX), currY);
                        prevData = (double)ie.Current;

                        if (dc.DisplayEvents)
                        {

                            string[] EventText = dc.GetEvents(i);
                            if (EventText != null)
                                g.FillEllipse(new SolidBrush(dc.Color), (int)Math.Round((i + 1) * _OffsetX) - (_MarkerSize / 2), currY - (_MarkerSize / 2), _MarkerSize, _MarkerSize);


                        }
                    }
                }
            }
        }


        // Drawing a marker at the nearest graph value to the mouse pointer
        private void DrawMarker(MouseEventArgs e)
        {
            if (_MouseDown)
            {
                this.Refresh();
                int xOffsetLocation = (int)Math.Round(e.X / _OffsetX);
                double MinDistance = int.MaxValue;
                int MinDistancePosition = 0;
                bool MarkerFound = false;
                double MinDistanceValue = 0;
                string MinDistanceName = "";
                string[] MinDistanceEvents = null;
                // Looking for the nearest graph and point in time
                foreach (DataChannel dc in _DataSources)
                {
                    if (dc.Count > xOffsetLocation && xOffsetLocation >= 0 && dc.Display)
                    {
                        double CurrentValue = (double)(dc.ToArray())[xOffsetLocation];
                        int CurrentYPosition = CalculateY(CurrentValue);
                        int CurrentDistance = Math.Abs(e.Y - CurrentYPosition);

                        if (CurrentDistance < MinDistance)
                        {
                            MinDistance = CurrentDistance;
                            MinDistancePosition = CurrentYPosition;
                            MinDistanceValue = CurrentValue;
                            MinDistanceName = dc.Name;
                            MinDistanceEvents = dc.GetEvents(xOffsetLocation);
                        }
                        MarkerFound = true;
                    }
                }
                // When point and graph found, draw the marker
                if (MarkerFound)
                {
                    string DrawString = String.Format("Good: " + MinDistanceName + ", Price: {0:c}", MinDistanceValue);

                    if (MinDistanceEvents != null)
                    {
                        // adding events to info to be displayed
                        DrawString += "\n";
                        for (int i = 0; i < MinDistanceEvents.Length; i++)
                            DrawString += " + " + MinDistanceEvents[i] + "\n";
                    }

                    Graphics g = this.CreateGraphics();
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int X = (int)Math.Round((xOffsetLocation + 1) * _OffsetX);
                    int Y = MinDistancePosition;
                    int MSH = MarkerSize / 2; //Marker Size half

                    g.DrawEllipse(new Pen(Brushes.DarkRed, GraphThickness), X - MSH, Y - MSH, MarkerSize, MarkerSize);

                    SizeF size = g.MeasureString(DrawString, new Font("Arial", 7));

                    // Making sure that infobox doesn't exceeds control drawing area
                    if ((Y + size.Height) > Height)
                        Y -= ((int)Math.Round(size.Height) + MarkerSize);

                    if ((X + size.Width) > Width)
                        X -= ((int)Math.Round(size.Width) + MarkerSize);

                    // Drawing info
                    g.FillRectangle(Brushes.LightYellow, X + MSH, Y + MSH, size.Width, size.Height);
                    g.DrawRectangle(new Pen(Color.Black), X + MSH, Y + MSH, size.Width, size.Height);
                    g.DrawString(DrawString, new Font("Arial", 7), Brushes.Black, X + MSH, Y + MSH);
                }
            }
        }

        // TODO: Implement mouse wheel times span change
        #region Mouse event overrides

        protected override void OnMouseMove(MouseEventArgs e)
        {
            DrawMarker(e);
            base.OnMouseMove(e);
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            DrawMarker(e);
            _MouseDown = false;
            base.OnMouseUp(e);
        }


        protected override void OnMouseLeave(EventArgs e)
        {
            _MouseDown = false;
            base.OnMouseLeave(e);
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            _MouseDown = true;
            base.OnMouseDown(e);
        }
        #endregion
    }
}