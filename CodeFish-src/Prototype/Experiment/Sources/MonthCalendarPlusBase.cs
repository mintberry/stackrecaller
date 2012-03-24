using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

namespace MonthCalendarPlusNameSpace
{
    public partial class MonthCalendarPlusBase : Control
    {
        public MonthCalendarPlusBase(DateTime m)
        {
            Width = CALENDAR_WIDTH;
            Height = CALENDAR_HEIGHT;

            InitializeComponent();

            month = new DateTime(m.Year, m.Month, 1); ;
            monthFont = new Font("Tahoma", 8f, FontStyle.Bold);
            monthFontColor = Color.Black;
            titleBackgroundColor = Color.LightGray;
            backgroundColor = Color.White;
            weekDayNamesColor = Color.LightGray;
            datesFont = new Font("Tahoma", 8f);
            borderColor = Color.White;
            datesColor = Color.Black;
            boldedDates = new List<DateTime>();

            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            LoadDates();

        }

        private const int CALENDAR_WIDTH = 156;
        private const int CALENDAR_HEIGHT = 140;
        private const int TITLE_HEIGHT = 30;
        private const int WEEKDAY_NAMES_HEIGHT = 10;

        StringFormat sf = new StringFormat();
        private List<Day> dates = new List<Day>();

        #region Props
        private List<DateTime> boldedDates;

        public List<DateTime> BoldedDates
        {
            get { return boldedDates; }
            set
            {
                List<DateTime> d = new List<DateTime>();
                foreach (DateTime dt in value)
                {
                    d.Add(dt.Date);
                }
                boldedDates = d;

                LoadDates();

            }

        }

        private LocationIndicator daysNotInMonthToDraw;

        public LocationIndicator DaysNotInMonthToDraw
        {
            get { return daysNotInMonthToDraw; }
            set
            {
                daysNotInMonthToDraw = value;
                LoadDates();
            }
        }

        private DateTime month;

        public DateTime Month
        {
            get { return month; }
        }

        private Color datesColor;

        public Color DatesColor
        {
            get { return datesColor; }
            set { datesColor = value; }
        }

        private Color borderColor;

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }

        private Font datesFont;

        public Font DatesFont
        {
            get { return datesFont; }
            set { datesFont = value; }
        }

        private Color backgroundColor;

        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        private Font monthFont;

        public Font MonthFont
        {
            get { return monthFont; }
            set { monthFont = value; }
        }

        private Color monthFontColor;

        public Color MontFontColor
        {
            get { return monthFontColor; }
            set { monthFontColor = value; }
        }
        private Color titleBackgroundColor;

        public Color TitleBackgroundColor
        {
            get { return titleBackgroundColor; }
            set { titleBackgroundColor = value; }
        }
        private Color weekDayNamesColor;

        public Color WeekDayNamesColor
        {
            get { return weekDayNamesColor; }
            set { weekDayNamesColor = value; }
        }

        #endregion

        public delegate void OnDateSelected(Day date, MonthCalendarPlusBase calendar);
        public event OnDateSelected DateSelected;

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            switch (HitTest(e.X, e.Y))
            {
                case HitArea.Title:
                    break;
                case HitArea.WeekDays:
                    break;
                case HitArea.Date:
                    DeselectAll();
                    Day da = GetDateOnLocation(e.X, e.Y);
                    da.Selected = true;
                    if (DateSelected != null)
                        DateSelected(da, this);
                    //Debug.WriteLine(da.Date.ToString());
                    break;
                default:
                    break;
            }
            this.Refresh();


        }
        public void DeselectAll()
        {
            foreach (Day d in dates)
            {
                d.Selected = false;
            }
            this.Refresh();
        }

        private Day GetDateOnLocation(int x, int y)
        {
            int w = 2;
            int h = TITLE_HEIGHT + WEEKDAY_NAMES_HEIGHT + 7;

            float wSpace = (Width - w) / 7f;
            float hSpace = (Height - h) / 6f;

            x -= w;
            y -= h;

            int x1 = (int)Math.Floor(x / wSpace);
            int y1 = (int)Math.Floor(y / hSpace);

            int index = x1 + (y1 * 7);

            return dates[index];

        }

        private HitArea HitTest(int x, int y)
        {
            if (y <= TITLE_HEIGHT + 2)
                return HitArea.Title;
            if (y > TITLE_HEIGHT && y <= TITLE_HEIGHT + WEEKDAY_NAMES_HEIGHT + 5)
                return HitArea.WeekDays;
            else
                return HitArea.Date;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            DrawBorder(pe.Graphics, new Rectangle(new Point(0, 0), new Size(CALENDAR_WIDTH - 1, CALENDAR_HEIGHT - 1)));
            DrawBackground(pe.Graphics, new Rectangle(new Point(1, 1), new Size(CALENDAR_WIDTH - 2, CALENDAR_HEIGHT - 2)));
            DrawTitle(pe.Graphics, new Rectangle(new Point(1, 1), new Size(CALENDAR_WIDTH - 2, TITLE_HEIGHT)));
            DrawWeekdayNames(pe.Graphics, new Rectangle(new Point(1, TITLE_HEIGHT + 2), new Size(CALENDAR_WIDTH - 2, WEEKDAY_NAMES_HEIGHT)));
            DrawDates(pe.Graphics, new Rectangle(new Point(1, TITLE_HEIGHT + WEEKDAY_NAMES_HEIGHT + 4), new Size(CALENDAR_WIDTH - 2, CALENDAR_HEIGHT - WEEKDAY_NAMES_HEIGHT - TITLE_HEIGHT - 4)));

            // Calling the base class OnPaint
            base.OnPaint(pe);

        }

        private void DrawDates(Graphics graphics, Rectangle rectangle)
        {
            foreach (Day d in dates)
            {
                DrawDate(graphics, d);
            }
        }

        private void DrawDate(Graphics graphics, Day d)
        {
            if (d.Draw)
            {

                if (d.Selected)
                {
                    graphics.FillRectangle(Brushes.Lavender, new Rectangle(d.Location, new Size(d.Size.Width - 1, d.Size.Height - 1)));
                    //graphics.DrawRectangle(Pens.GreenYellow,new Rectangle(d.Location, d.Size));
                }
                Font f;
                if (d.Bolded)
                    f = new Font(datesFont, FontStyle.Bold);
                else
                    f = datesFont;

                if (d.Grayed)
                    graphics.DrawString(d.DateString, f, new SolidBrush(this.weekDayNamesColor), new Rectangle(d.Location, d.Size), sf);
                else
                    graphics.DrawString(d.DateString, f, new SolidBrush(this.datesColor), new Rectangle(d.Location, d.Size), sf);

                if (d.Date.Date == DateTime.Today)
                    graphics.DrawRectangle(Pens.Red, new Rectangle(d.Location, new Size(d.Size.Width - 2, d.Size.Height - 2)));
            }
        }

        private void DrawBackground(Graphics graphics, Rectangle rectangle)
        {
            graphics.FillRectangle(new SolidBrush(backgroundColor), rectangle);
        }



        private void LoadDates()
        {
            string[] weekdays = GetWeekdays();
            string lblDay;

            List<Day> days = new List<Day>();

            lblDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(month.DayOfWeek);

            int i = 0;
            for (i = 0; i < weekdays.Length; i++)
            {
                if (weekdays[i] == lblDay)
                    break;
            }

            for (int j = 0; j < 42; j++)
                days.Add(new Day(new DateTime(month.Year, month.Month, 1).AddDays(((7 - i) - 7 + j))));

            float w = 0;
            float wSpace = Width / 7f;
            float h = TITLE_HEIGHT + WEEKDAY_NAMES_HEIGHT + 5;
            float hSpace = (Height - TITLE_HEIGHT - WEEKDAY_NAMES_HEIGHT - 5) / 6f;

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    int index = y + (x * 7);
                    Day d = days[index];
                    if (boldedDates.Contains(d.Date.Date))
                        d.Bolded = true;


                    d.Location = new Point((int)Math.Round(w), (int)Math.Round(h));
                    d.Size = new Size((int)Math.Round(wSpace), (int)Math.Round(hSpace));
                    if (index < i || index > (i + DateTime.DaysInMonth(month.Year, month.Month) - 1))
                        d.Grayed = true;

                    switch (daysNotInMonthToDraw)
                    {
                        case LocationIndicator.Previous:
                            if (index > (i + DateTime.DaysInMonth(month.Year, month.Month) - 1))
                                d.Draw = false;
                            break;
                        case LocationIndicator.Next:
                            if (index < i)
                                d.Draw = false;
                            break;
                        case LocationIndicator.None:
                            if (index < i || index > (i + DateTime.DaysInMonth(month.Year, month.Month) - 1))
                                d.Draw = false;
                            break;
                        default:
                            break;
                    }

                    w += wSpace;
                }
                w = 0;
                h += hSpace;
            }
            dates = days;

        }

        private string[] GetWeekdays()
        {
            DateTimeFormatInfo d = CultureInfo.CurrentCulture.DateTimeFormat;
            int FirstDayOfWeek = (int)d.FirstDayOfWeek;

            int index = 0;
            string[] sysNames = d.AbbreviatedDayNames;
            string[] weekdays = new string[7];

            weekdays.Initialize();

            // Arrange weekdays starting with first day of week
            for (int i = FirstDayOfWeek; i < weekdays.Length; i++)
            {
                weekdays[index] = sysNames[i];
                index++;
            }
            for (int i = 0; i < FirstDayOfWeek; i++)
            {
                weekdays[index] = sysNames[i];
                index++;
            }

            return weekdays;
        }

        private void DrawWeekdayNames(Graphics graphics, Rectangle rectangle)
        {

            string[] weekdays = GetWeekdays();

            float o = (float)rectangle.X;
            float space = rectangle.Width / 7f;

            foreach (string s in weekdays)
            {
                graphics.DrawString(s, datesFont, new SolidBrush(weekDayNamesColor), new Rectangle(new Point((int)Math.Round(o), rectangle.Y), new Size((int)Math.Round(space), rectangle.Height)), sf);
                o += space;
            }
            graphics.DrawLine(new Pen(weekDayNamesColor), rectangle.Left + 2, rectangle.Bottom + 2, rectangle.Right - 2, rectangle.Bottom + 2);

        }

        private void DrawTitle(Graphics graphics, Rectangle rectangle)
        {
            graphics.FillRectangle(new SolidBrush(titleBackgroundColor), rectangle);

            graphics.DrawString(String.Format("{0:Y}", month), monthFont, new SolidBrush(monthFontColor), rectangle, sf);

        }

        private void DrawBorder(Graphics graphics, Rectangle rectangle)
        {
            graphics.DrawRectangle(new Pen(borderColor), rectangle);
        }
    }

    public enum HitArea { Title, WeekDays, Date };
}