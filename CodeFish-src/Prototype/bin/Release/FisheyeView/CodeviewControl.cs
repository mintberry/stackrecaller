using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using DDW;
using DDW.Collections;
using System.Runtime.InteropServices;

namespace Prototype
{
    public partial class CodeviewControl : UserControl
    {
        #region Structures and enums

        public enum Offset
        {
            Top,
            Middle,
            Bottom
        };

        private struct SegmentInfo
        {
            public int start;
            public int end;
            public float weight;
            public SegmentInfo(int s, int e, float w)
            {
                start = s;
                end = e;
                weight = w;
            }

        }
        #endregion

        #region Fields

        // Selector
        //private int _selectedLine = 0;
        private int _overviewSelectedLine = 0;


        //Layout
        private Rectangle _overviewRectangle;
        private Rectangle _connectionsRectangle;
        private Rectangle _codeviewRectangle;
        private Layout _layout;
        private Rectangle _slackRectangle;


        //Overview related fields
        private Bitmap _renderedOverview;
        private bool _mouseButtonDown = false;

        //Connectings fields
        Pen _selectedPen = new Pen(Settings.Default.SelectedColor, 2.0f);

        //Timer
        Timer depreciationTimer = new Timer();
        #endregion

        //Constructor
        public CodeviewControl()
        {
            InitializeComponent();
            UpdateLayoutRectangles();
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.CacheText, false);
            SetStyle(ControlStyles.ContainerControl, false);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Settings.Default.StringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            View.Default.OnViewRefresh += new View.ViewRefreshDelegate(OnViewRefresh);
            Model.Default.OnModelChanged += new Model.ModelChangedDelegate(Default_OnModelChanged);
            Settings.Default.Update += new Settings.OnSettingsUpdate(OnSettingsUpdate);

            Application.Idle += new EventHandler(Application_Idle);
        }


        #region Idle message loop
        [StructLayout(LayoutKind.Sequential)]
        public struct PeekMsg
        {
            public IntPtr hWnd;
            public Message msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out PeekMsg msg, IntPtr hWnd,
                uint messageFilterMin, uint messageFilterMax, uint flags);
        
        void Application_Idle(object sender, EventArgs e)
        {
            while (IsIdle)
            {
                View.Default.Refresh();
            }
        }

        public bool IsIdle
        {
            get
            {
                PeekMsg msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
        #endregion


        void Default_OnModelChanged()
        {
            _renderedOverview = null;
        }

        public void OnViewRefresh()
        {
            Refresh();
        }

        public void OnSettingsUpdate()
        {
        }

        #region Focus manipulation
        private void SetFocus(int center, bool animate, Logger.EntryType entryType)
        {
            if (Model.Default.Document == null || Model.Default.Document.Length == 0)
            {
                return; 
            }
            else
            {
                int newcenter;

                if (center >= Model.Default.Document.Length)
                    newcenter = Model.Default.Document.Length - 1;
                else if (center < 0)
                    newcenter = 0;
                else
                    newcenter = center;

                Logger.Default.Log(entryType, null);
                Model.Default.UpdateFocus(newcenter, animate);
            }
        }

        private void MoveFocus(int delta, bool animate, Logger.EntryType entryType)
        {
            SetFocus(Model.Default.Focus.Center + delta, animate, entryType);
        }
        #endregion

        #region Mouse and keyboard interaction
        
        DateTime _lastInteraction = DateTime.Now;
        double _interactionTimeOut = 0.1;

        private bool ShouldAnimate()
        {
            bool result = (DateTime.Now - _lastInteraction).TotalSeconds > _interactionTimeOut;
            _lastInteraction = DateTime.Now;
            return result;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta < 0)
                MoveFocus(1, ShouldAnimate(), Logger.EntryType.Scroll);
            else if (e.Delta > 0)
                MoveFocus(-1, ShouldAnimate(), Logger.EntryType.Scroll);

            base.OnMouseWheel(e);
        }

        private void OverviewFocusAreaInteraction(Point p, bool animate)
        {
            if (_overviewRectangle.Contains(p))
            {
                int selectedLine = TransformFromScaled(p.Y);
                if(animate)
                    SetFocus(selectedLine, animate, Logger.EntryType.Click);
                else
                    SetFocus(selectedLine, animate, Logger.EntryType.Scroll);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            System.Diagnostics.Trace.WriteLine("on mouse click2");
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _mouseButtonDown = true;
            OverviewFocusAreaInteraction(e.Location, true);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseButtonDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseButtonDown)
                OverviewFocusAreaInteraction(e.Location, false);

            if (_overviewRectangle.Contains(e.Location))
            {
                _overviewSelectedLine = e.Y;
                //if(!_mouseButtonDown) this.Refresh();
                return;
            }
            else if (_overviewSelectedLine != -1)
            {
                _overviewSelectedLine = -1;

            }

            if (Model.Default.SelectedLine != -1 && !_codeviewRectangle.Contains(e.Location))
                Model.Default.SelectedLine = -1;

            base.OnMouseMove(e);
        }

        //OnKeyDown does not trap arrow keys - argh!
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool handled = false;
            
            switch (keyData)
            {
                case Keys.Down:
                    MoveFocus(1, ShouldAnimate(), Logger.EntryType.KeyPress);
                    handled= true;
                    break;

                case Keys.Up:
                    MoveFocus(-1, ShouldAnimate(), Logger.EntryType.KeyPress);
                    handled= true;
                    break;

                case Keys.PageDown:
                    MoveFocus(10, ShouldAnimate(), Logger.EntryType.KeyPress);
                    handled= true;
                    break;

                case Keys.PageUp:
                    MoveFocus(-10, ShouldAnimate(), Logger.EntryType.KeyPress);
                    handled= true;
                    break;

                case Keys.End:
                    SetFocus(Model.Default.Document.Length - 1, true, Logger.EntryType.KeyPress);
                    handled= true;
                    break;

                case Keys.Home:
                    SetFocus(0, true, Logger.EntryType.KeyPress);
                    handled= true;
                    break;
            }
            
            if (handled)
            {
                Logger.Default.Log(Logger.EntryType.KeyPress, null);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Layout
        protected override void OnResize(EventArgs e)
        {
            UpdateLayoutRectangles();

            scrollableDX.Location = _codeviewRectangle.Location;
            _codeviewRectangle.Width = Math.Max(_codeviewRectangle.Width, 10);
            _codeviewRectangle.Height = Math.Max(_codeviewRectangle.Height, 10);
            scrollableDX.Size = _codeviewRectangle.Size;

            //this.Refresh();
            base.OnResize(e);
        }

        private void UpdateLayoutRectangles()
        {
            _codeviewRectangle = new Rectangle(0, 0, this.Width - Settings.OverviewWidth - Settings.ConnectionsAreaWidth, this.Height);
            _connectionsRectangle = new Rectangle(this.Width - Settings.OverviewWidth - Settings.ConnectionsAreaWidth, 0, Settings.ConnectionsAreaWidth, this.Height - scrollableDX.ScroolBarHeight);
            _overviewRectangle = new Rectangle(this.Width - Settings.OverviewWidth, 0, Settings.OverviewWidth, this.Height-scrollableDX.ScroolBarHeight);
            _slackRectangle = new Rectangle(_connectionsRectangle.X, _overviewRectangle.Height, _overviewRectangle.Width+_connectionsRectangle.Width, scrollableDX.ScroolBarHeight);

            View.Default.Size = new Size(_codeviewRectangle.Width, _codeviewRectangle.Height - scrollableDX.ScroolBarHeight);

        }
        #endregion
        
        #region Segment handling
        private void AddSegment(int start, int end, float segmentWeight, ref List<SegmentInfo> segments, ref float maxSegmentWeight)
        {
            Debug.Assert(!float.IsNaN(segmentWeight));
            maxSegmentWeight = Math.Max(maxSegmentWeight, segmentWeight);
            segments.Add(new SegmentInfo(start, end, segmentWeight));

        }

        // Draw all segment connectors, color coded with weight relative to maximum weight
        private void DrawSegmentConnectors(Graphics g, List<SegmentInfo> segments, float maxSegmentWeight)
        {
            foreach (SegmentInfo si in segments)
                DrawSegmentConnector(g, si.start, si.end, si.weight / (si.end - si.start + 1));
        }

        private void DrawSegmentConnector(Graphics g, int segmentStartIndex, int segmentEndIndex, float weight)
        {
            PointF[] endB = GetBezierPoints(segmentEndIndex, Offset.Bottom);
            PointF[] startB = GetBezierPoints(segmentStartIndex, Offset.Top);
            GraphicsPath gp = new GraphicsPath();

            gp.StartFigure();
            gp.AddLine(startB[0], endB[0]);
            gp.AddBezier(endB[0], endB[1], endB[2], endB[3]);
            gp.AddLine(endB[3], startB[3]);
            gp.AddBezier(startB[3], startB[2], startB[1], startB[0]);
            gp.CloseFigure();

            gp.StartFigure();
            gp.AddRectangle(new RectangleF(startB[3], new SizeF(_overviewRectangle.Width, endB[3].Y - startB[3].Y)));
            gp.CloseFigure();

            Color background = ColorUtilities.Interpolate(Settings.Default.OverviewBackgroundColor, Settings.Default.ConnectorsColor, 0.2f);
            Color color = ColorUtilities.Interpolate(background, Settings.Default.ConnectorsColor, weight);

            g.FillPath( new SolidBrush(color), gp);

            if (Model.Default.SelectedLine != -1 && Model.Default.SelectedLine >= segmentStartIndex && Model.Default.SelectedLine <= segmentEndIndex)
            {
                g.DrawBeziers(_selectedPen, GetBezierPoints(Model.Default.SelectedLine, Offset.Middle));
                int y = TransformToScaled(Model.Default.SelectedLine);
                g.DrawLine(_selectedPen, _overviewRectangle.X, y, this.Width, y);
            }

        }

        private PointF[] GetBezierPoints(int index, Offset ot)
        {
            PointF[] points = new PointF[4];


            float ratio = 0.75f;
            float overviewPos = TransformToScaled(index);
            LineLayout ll = _layout.lines[index];
            float offset = ll.height / 2;

            switch (ot)
            {
                case Offset.Top:
                    offset = 0f;
                    break;
                case Offset.Middle:
                    offset = ll.height / 2;
                    break;
                case Offset.Bottom:
                    offset = ll.height;
                    break;
                default:
                    offset = 0f;
                    break;
            }

            PointF p1 = new PointF(_connectionsRectangle.X, (float)ll.y + offset);
            PointF p2 = new PointF(_overviewRectangle.X - (Settings.ConnectionsAreaWidth / 4), ratio * ((float)ll.y + offset) + (1f - ratio) * overviewPos);

            PointF p3 = new PointF(_connectionsRectangle.X + (Settings.ConnectionsAreaWidth / 4), (1f - ratio) * (float)((ll.y + offset) + (ll.height / 2)) + ratio * overviewPos);
            PointF p4 = new PointF(_overviewRectangle.X, overviewPos);

            points[0] = p1;
            points[1] = p2;
            points[2] = p3;
            points[3] = p4;

            return points;

        }
        #endregion

        #region Transformation of coordinates
        private int TransformToScaled(int index)
        {
            if (Model.Default.Document == null || Model.Default.Document.Length == 0)
                return 0;
            else
                return (_overviewRectangle.Height * index) / Model.Default.Document.Length;
        }

        private int TransformFromScaled(int y)
        {
            return (y * Model.Default.Document.Length) / _overviewRectangle.Height;
        }
        #endregion

        #region Painting
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Model.Default.DOIStrategy == null || Model.Default.RenderStrategy == null || Model.Default.Document == null || View.Default.Layout == null)
            {
                //base.OnPaint(e);
                return;
            }

            _layout = View.Default.Layout;

            Graphics g = e.Graphics;
            Size s = this.Size;

            // Draw background
            using (Brush backBrush = new SolidBrush(Settings.Default.OverviewBackgroundColor))
            {
                Rectangle u = Rectangle.Union(_overviewRectangle, _connectionsRectangle);
                g.FillRectangle(backBrush, u);
                g.FillRectangle(backBrush, _slackRectangle);
            }

            // Drawing focus area rectangle in overview
            int fTop = TransformToScaled(Model.Default.Focus.Start);
            Rectangle focusAreaRectangle = new Rectangle(_overviewRectangle.X, fTop, Settings.OverviewWidth - 1, TransformToScaled(Model.Default.Focus.End) - fTop);
            g.FillRectangle(new SolidBrush(Settings.Default.FocusColor), focusAreaRectangle);

            DrawConnectors(g);

            if (_overviewSelectedLine != -1)
            {
                g.DrawLine(_selectedPen, _overviewRectangle.X, _overviewSelectedLine, this.Width, _overviewSelectedLine);
            }

            DrawOverview(e);
        }

        private void DrawConnectors(Graphics g)
        {
            int fontHeight = Settings.Default.Font.Height;
            int end = 0;
            int lastDrawnLine = -1;
            int segmentStartIndex = -1;
            float segmentWeight = 0f;
            List<SegmentInfo> segments = new List<SegmentInfo>();
            float maxSegmentWeight = 0.001f;

            for (int i = 0; i < _layout.lines.Length; i++)
            {
                LineLayout line = _layout.lines[i];
                if (line.shown)
                {
                    if (segmentStartIndex == -1)
                        segmentStartIndex = i;

                    if (lastDrawnLine == -1)
                        lastDrawnLine = i;

                    end = i;


                    segmentWeight += ColorUtilities.Interpolate((float)line.doi.importance, 1f, line.focus);

                    if (i >= lastDrawnLine + Settings.Default.SegmentFuzziness)
                    {
                        AddSegment(segmentStartIndex, lastDrawnLine, segmentWeight, ref segments, ref maxSegmentWeight);
                        segmentStartIndex = i;
                        segmentWeight = 0f;
                    }

                    lastDrawnLine = i;
                }
            }

            // Add last segment connector
            AddSegment(segmentStartIndex,end, segmentWeight, ref segments, ref maxSegmentWeight);

            // Draw all segment connectors
            DrawSegmentConnectors(g, segments, maxSegmentWeight);
        }

        private void DrawOverview(PaintEventArgs e)
        {
            if (_renderedOverview == null || _renderedOverview.Size != _overviewRectangle.Size)
            {
                _renderedOverview = new Bitmap(_overviewRectangle.Size.Width, _overviewRectangle.Size.Height);
                Graphics g = Graphics.FromImage(_renderedOverview);

                for (int i = 0; i < Model.Default.Document.Length; i++)
                {
                    float x = 0;
                    for (int j = 0; j < Model.Default.TokenizedDocument[i].Length; j++)
                    {
                        string tokenValue = Model.Default.TokenizedDocument[i][j].Value.Replace("\t", "    ");
                        int trimmedLength = tokenValue.TrimStart().Length;
                        int whiteSpace = tokenValue.Length - tokenValue.TrimStart().Length;
                        x += whiteSpace;

                        using (Brush b = new SolidBrush(ColorUtilities.ToOverviewColor(Model.Default.TokenizedDocument[i][j].Color)))
                            g.FillRectangle(b, x, TransformToScaled(i), trimmedLength, 1);

                        x += trimmedLength;
                    }
                }
            }
            e.Graphics.DrawImage(_renderedOverview, _overviewRectangle.Location);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            return;
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            //View.Default.Refresh();
        }
    }
}
