using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Prototype
{
    public enum Seperators
    {
        SolidLine,
        LinenumberIndication
    };

    class Settings
    {
        public delegate void OnSettingsUpdate();
        public event OnSettingsUpdate Update;

        static private Settings settings = null;
        private StringFormat _stringFormat = StringFormat.GenericTypographic;
        private static Color DEFAULT_SELECTED_COLOR = Color.FromArgb(168, 205, 241);
        private Color _backgroundColor = System.Drawing.SystemColors.Window;


        private Color _focusCenterColor = Color.BlanchedAlmond;
        private Color _focusColor = Color.Cornsilk;
        private Color _selectedColor = DEFAULT_SELECTED_COLOR;
        private Color _seperatorColor = Color.LightGray;
        private Color _connectorsColor = Color.FromArgb(223, 223, 223);
        private Color _overviewBackgroundColor = Color.GhostWhite;
        private Brush _overviewCodeBrush = Brushes.LightGray;
        private int _segmentFuzziness = 4;
        private Seperators _segmentSeperatorStyle = Seperators.SolidLine;
        private bool _drawSeperator = true;
        private int _whitespaceWidth = 8;

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; this.Refresh(); }
        }

        public int WhitespaceWidth
        {
            get { return _whitespaceWidth; }
            set { _whitespaceWidth = value; this.Refresh(); }
        }


        // Font
        private Font _font = new Font("Courier New", 11);

        public Font Font
        {
            get { return _font; }
            set { _font = value; this.Refresh(); }
        }

        private int _minFontSize = 7;

        public const int NumberingWidth = 50;
        public const int OverviewWidth = 150;
        public const int ConnectionsAreaWidth = 50;

        public StringFormat StringFormat
        {
            get { return _stringFormat; }
        }

        public Color FocusCenterColor
        {
            get { return _focusCenterColor; }
            set { _focusCenterColor = value; this.Refresh(); }
        }

        private Color _semanticColor = Color.AliceBlue;

        public Color SemanticColor
        {
            get { return _semanticColor; }
            set { _semanticColor = value; this.Refresh(); }
        }

        public Color FocusColor
        {
            get { return _focusColor; }
            set { _focusColor = value; this.Refresh(); }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; this.Refresh(); }
        }

        public Color SeperatorColor
        {
            get { return _seperatorColor; }
            set { _seperatorColor = value; this.Refresh(); }
        }

        public Color ConnectorsColor
        {
            get { return _connectorsColor; }
            set { _connectorsColor = value; this.Refresh(); }
        }

        public Color OverviewBackgroundColor
        {
            get { return _overviewBackgroundColor; }
            set { _overviewBackgroundColor = value; this.Refresh(); }
        }

        public Brush OverviewCodeBrush
        {
            get { return _overviewCodeBrush; }
            set { _overviewCodeBrush = value; this.Refresh(); }
        }

        public int SegmentFuzziness
        {
            get { return _segmentFuzziness; }
            set { _segmentFuzziness = value; this.Refresh(); }
        }

        public Seperators SegmentSeperatorStyle
        {
            get { return _segmentSeperatorStyle; }
            set { _segmentSeperatorStyle = value; this.Refresh(); }
        }


        public bool DrawSeperator
        {
            get { return _drawSeperator; }
            set { _drawSeperator = value; this.Refresh(); }
        }

        public int MinFontSize
        {
            get { return _minFontSize; }
            set
            {
                _minFontSize = value;
                this.Refresh();
            }
        }

        public int MinFontHeight
        {
            get
            {
                Font f = new Font(_font.FontFamily, _minFontSize);
                return f.Height;
            }
        }

        private void Refresh()
        {
            if (Update != null) Update();
        }

        #region Singleton
        protected Settings()
        {

        }

        public static Settings Default
        {
            get
            {
                if (settings == null)
                    settings = new Settings();

                return settings;
            }
        }
        #endregion
    }
}
