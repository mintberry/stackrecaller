#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX;
using UMD.HCIL.Piccolo;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Activities;
using System.Globalization;
#endregion

namespace SemanticZoomSheet
{
    #region Alignment enum
    public enum Alignment
    {
        Left, Right, Center
    };
    #endregion

    #region ScaleRepresentation struct
    public class ScaleRepresentation
    {
        public string Text;
        public int ColSpan;

        public ScaleRepresentation(string t, int c)
        {
            Text = t;
            ColSpan = c;
        }
        public override string ToString()
        {
            return "{Text=" + Text + ", ColSpan=" + ColSpan.ToString();
        }
        public override bool Equals(object obj)
        {
            ScaleRepresentation sr = (ScaleRepresentation)obj;
            return (sr.Text == this.Text && sr.ColSpan == this.ColSpan);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    #endregion

    public class Cell : PNode
    {
        #region Constants and statics
        public const int STD_CELL_WIDTH = 120;
        public const int STD_CELL_HEIGHT = 20;
        public const int STD_CELL_TITLE_WIDTH = 250;
        public const float DOI_THRESHOLD = 1f;

        public static Font DEFAULT_FONT = new Font("Arial", 10f);
        #endregion

        CultureInfo culture = new CultureInfo("da-DK");

        #region Events
        //public delegate void OnAnimationEndedDelegate();
        //public event OnAnimationEndedDelegate OnAnimationEnded;
        void Cell_MouseEnter(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
           MainForm.Instance.UpdateCell(this.RealValuesToString());
           fillBrush = hoverBrush;
           if (HeaderCell != null)
           {
               HeaderCell.fillBrush = fillBrush;
               HeaderCell.InvalidatePaint();
           }
           InvalidatePaint();
           //e.Handled = false;
           
        }
        void Cell_MouseLeave(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            fillBrush = (Selected ? lightBlueBrush : whiteBrush);
            if (HeaderCell != null)
            {
                HeaderCell.fillBrush = whiteBrush;
                HeaderCell.InvalidatePaint();
            }
            InvalidatePaint();
            //e.Handled = false;
        }
        #endregion

        #region Fields

        //Interesting data
        public float Value = float.NaN;
        public float DOI = SemanticZoomSheetZoomEventHandler.DEFAULT_MIN_SCALE / 2;

        //References
        public Cell LeftCell = null;
        public Cell RightCell = null;
        public HeaderCell HeaderCell = null;

         //Brushes and fonts
        internal Font CellFont = Cell.DEFAULT_FONT;
        internal Brush textBrush = Brushes.Black;
        internal Pen blackPen = new Pen(Color.FromArgb(192,192,192));
        internal Brush whiteBrush = new SolidBrush(Color.FromArgb(252,252,252));
        internal Brush lightBlueBrush = new SolidBrush(Color.FromArgb(240,245,255));
        internal Brush fillBrush;
        internal Brush hoverBrush = Brushes.Lavender;
        

        //Drawinbg text
        public bool DrawText = true;
        private Size TextMeasure = new Size();
        private Brush TextBoxColor = Brushes.Gainsboro;
        internal const int TEXT_OFFSET_Y = 2;  //(CELL_HEIGHT - FONT_HEIGHT) / 2 = (20 - 16) / 2

        //Representation handling
        internal ScaleRepresentation currentRepresentation;
        internal string DefaultText;

        //Interaction
        private bool selected = false;
        public bool Selected
        {
            get { return selected; }
            set {
                
                selected = value;
                fillBrush = (value ? lightBlueBrush : whiteBrush);
            }
        }

        public ScaleRepresentation CurrentRepresentation
        {
            get { return currentRepresentation; }
            set
            {
                currentRepresentation = value;
                TextMeasure = TextRenderer.MeasureText(CurrentRepresentation.Text, CellFont);
                if (!Visible)
                    Show();
                else
                    InvalidatePaint();
            }
        }

        public float ChangeFromLeft
        {
            get
            {
                if (LeftCell != null)
                    return Value - LeftCell.Value;
                else
                    return 0f;
            }
        }

        public float ChangeFromRight
        {
            get
            {
                if (RightCell != null)
                    return RightCell.Value - Value;
                else
                    return 0f;
            }
        }
	
        #endregion

        #region Utils
        public Cell GetSpanEndCell(int c)
        {
            Cell rc = this.RightCell;
            for (int i = 1; i < c - 1; i++, rc = rc.RightCell) ;
            return rc;
        }
        public string ToFullString()
        {
            return ToString() + ": " + CurrentRepresentation.ToString();
        }
        private bool IsBetween(float a, float b, float num)
        {
            if (a > b)
                return (num <= a && num > b);
            else
                return (num > a && num <= b);
        }
        public void SetStyle(FontStyle s)
        {
            this.CellFont = new Font(this.CellFont, s);
        }
        public override string ToString()
        {
            return CurrentRepresentation.Text;
        }
        public float Average(int left, int right)
        {
            float sum = 0f;

            Cell lc = this.LeftCell;
            for (int l = 0; l < left; l++)
            {

                if (lc != null)
                {
                    sum += lc.Value;
                    lc = lc.LeftCell;
                }
                else
                {
                    sum += this.Value;
                }

            }

            Cell rc = this.RightCell;
            for (int r = 0; r < right; r++)
            {
                if (rc != null)
                {
                    sum += rc.Value;
                    rc = rc.RightCell;
                }
                else
                {
                    sum += this.Value;
                }
            }

            return ((sum + this.Value) / (float)(left + right + 1));

        }
        public string RealValuesToString()
        {
            string s = (currentRepresentation.ColSpan > 1 ? "(" + CurrentRepresentation.ColSpan + " celler) " : "") + GetFormattedString(Value);
            Cell rc = this.RightCell;
            for (int i = 0; i < CurrentRepresentation.ColSpan - 1; i++, rc = rc.RightCell)
            {
                s += ", " + GetFormattedString(rc.Value);
            }
            return s;
        }
        #endregion

        private string GetFormattedString(float f)
        {
            string t = f.ToString("#,#;(-#,#)", culture);
            if (t == "")
                t = "0";
            return t;
        }

        private string GetFormattedThousandsString(float f)
        {
            string t = f.ToString("#,#,;(-#,#,)", culture);
            if (t == "")
                t = "0";
            return t;
        }
        #region Ctor
        public Cell(float value, int width)
        {
           // whiteBrush = new SolidBrush(Color.FromArgb((int)(255 * SZSUtils.GetRandom()), (int)(255 * SZSUtils.GetRandom()), (int)(255 * SZSUtils.GetRandom())));

            string text = GetFormattedString(value);
            DefaultText = text;
            CurrentRepresentation = new ScaleRepresentation(text, 1);
            this.Value = value;
            this.Width = width;
            this.Height = STD_CELL_HEIGHT;
            this.CellFont = DEFAULT_FONT;
            
            
            this.Bounds = new RectangleF(0, 0, Cell.STD_CELL_WIDTH, Cell.STD_CELL_HEIGHT);

            TextMeasure = TextRenderer.MeasureText(CurrentRepresentation.Text, CellFont);

            fillBrush = whiteBrush;

            this.MouseEnter += new PInputEventHandler(Cell_MouseEnter);
            this.MouseLeave += new PInputEventHandler(Cell_MouseLeave);
        }

        
        #endregion

        #region Painting

        public virtual int GetTextXPos()
        {
            return (int)Math.Round(this.bounds.Right - TextMeasure.Width);
        }

        protected override void Paint(PPaintContext paintContext)
        {
            Graphics g = paintContext.Graphics;
            //Filling background
            g.FillRectangle(fillBrush, this.bounds);

            //Debug.WriteLine(Parent.MatrixReference.Elements[3]);
            //Drawing the text
            int XPos = GetTextXPos();
            if (DrawText)
                g.DrawString(CurrentRepresentation.Text, CellFont, textBrush, 
                    XPos, Cell.TEXT_OFFSET_Y);
            else
                g.FillRectangle(TextBoxColor, new Rectangle(new Point(XPos,Cell.TEXT_OFFSET_Y),TextMeasure));

            //Drawing border
            g.DrawRectangle(blackPen, Rectangle.Round(this.bounds));
            base.Paint(paintContext);
        }
        #endregion

        #region Compute Scale Representations
        #region Compute Representation text and real colspan
        private string GetValueRepresentation(int c)
        {
            return GetFormattedString(Value);
        }
        private string GetIntervalRepresentation(int c)
        {
            return GetFormattedThousandsString(Value) + " - " + GetFormattedThousandsString(GetSpanEndCell(c).Value);
        }
        private string GetTrendRepresentation(int c)
        {
            string t = "";
            float num1 = GetSpanEndCell(c).Value;
            float change = num1 / this.Value;
            
            if (float.IsNaN(change) || float.IsInfinity(change))
                t += " 0 %";
            else
                t += Math.Round((Math.Pow(change, 1 / (float)c)-1) * 100, 1).ToString() + "%";

            return t;
        }
        private string GetAverageRepresentation(int c)
        {
            Cell rc = this.RightCell;
            int i;
            string t = "M: ";
            float num1 = 0f;
            for (i = 0; i < c - 1; i++,rc = rc.RightCell)
                num1 += rc.Value;
            float avg = num1 / i;
            t += GetFormattedThousandsString((float)Math.Round(avg, 1));
            return t;
        }
        #endregion

        private const float LIMIT_0 = float.MaxValue;
        private const float LIMIT_1 = 0.9f;
        private const float LIMIT_2 = 0.85f;
        private const float LIMIT_3 = 0.80f;
        private const float LIMIT_4 = float.MinValue;

        public ScaleRepresentation GetRepresentationAtScale(float s, int c)
        {
            Cell rc = this.RightCell;
            string t = "";

            if (c <= 0)
                return null;
            if (c <= 1)
                t = GetValueRepresentation(c);
            else
            {
                if (IsBetween(LIMIT_0, LIMIT_1, s))
                    t = (c <= 1 ? GetValueRepresentation(c) : GetIntervalRepresentation(c));
                else if (IsBetween(LIMIT_1, LIMIT_2, s))
                    t = GetIntervalRepresentation(c);
                else if (IsBetween(LIMIT_2, LIMIT_3, s))
                    t = GetTrendRepresentation(c) + ", " + GetAverageRepresentation(c);
                else if (IsBetween(LIMIT_3, LIMIT_4, s))
                    t = GetTrendRepresentation(c);
            }
            return new ScaleRepresentation(t, c);
        }
        #endregion

        public void Hide()
        {
            Pickable = false;
            Visible = false;
        }

        public void Show()
        {
            Pickable = true;
            Visible = true;
        }

        internal void SetRepresentationAndWidth(ScaleRepresentation sr, float p)
        {
            
            currentRepresentation = sr;
            TextMeasure = TextRenderer.MeasureText(currentRepresentation.Text, CellFont);
            Width = p;
            if (!Visible)
                Show();
        }

    }
}