using System;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Diagnostics;


namespace Prototype
{
    public partial class DXLayoutControl : UserControl
    {
        #region Fields

        Device device = null;
        DateTime startTime = DateTime.Now;
        Font[] fonts;

        int _scrollableWidth;
        private int _hScrollValue;

        Layout _layout;

//        string _fontFamily = "Courier New";
        int _maxFontHeight;
        private const int NumberingWidth = 50;

        #endregion

        #region Properties

        public int HScrollValue
        {
            get { return _hScrollValue; }
            set {
                _hScrollValue = value; 
                Refresh(); 
            }
        }

        public int ScrollableWidth
        {
            get { return _scrollableWidth; }
        }

        public int ScrollableArea
        {
            get { return Math.Max(this.Size.Width - NumberingWidth, 0); }
        }
        #endregion

        public DXLayoutControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            if (!InitDevice())
                MessageBox.Show("Failed to init device.");

            if (!InitResources())
                MessageBox.Show("Failed to init resources.");

            Model.Default.OnModelChanged += new Model.ModelChangedDelegate(OnModelChanged);
            Settings.Default.Update += new Settings.OnSettingsUpdate(OnSettingsUpdate);
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            Logger.Default.Log(Logger.EntryType.Click, null);
            Model.Default.UpdateFocus(Model.Default.SelectedLine, true);
            base.OnMouseClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            if (!this.ClientRectangle.Contains(e.Location))
            {
                Model.Default.SelectedLine = -1;
                return;
            }

            if (_layout == null || _layout.lines == null)
                return;

            for (int i = 0; i < _layout.lines.Length; i++)
            {
                LineLayout ll = _layout.lines[i];
                if (ll.shown && e.Location.Y >= ll.y && e.Location.Y <= ll.y + ll.height)
                {
                    Model.Default.SelectedLine = i;
                    return;
                }
            }
        
            base.OnMouseMove(e);
        }

        public void OnModelChanged()
        {
            MeasureDocument();
        }

        public void OnSettingsUpdate()
        {
            InitResources();
        }

        private bool InitDevice()
        {
            try
            {
                PresentParameters parameters = new PresentParameters();
                
                parameters.BackBufferCount = 2;
                parameters.Windowed = true;
                parameters.SwapEffect = SwapEffect.Discard;

                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, parameters);

                device.DeviceLost += new EventHandler(OnDeviceLost);
                device.DeviceReset += new EventHandler(OnDeviceReset);
                device.Disposing += new EventHandler(OnDeviceDisposing);
                device.DeviceResizing += new System.ComponentModel.CancelEventHandler(OnDeviceResizing);

                return true;
            }
            catch (DirectXException)
            {
                return false;
            }
        }

        private bool InitResources()
        {
            // Dispose of font resources
            for (int i = 0; i < _maxFontHeight; i++)
            {
                if (fonts[i] != null)
                {
                    fonts[i].Dispose();
                    fonts[i] = null;
                }
            }

            _maxFontHeight = Settings.Default.Font.Height + 1;

            fonts = new Font[_maxFontHeight];

            for (int i = 0; i < _maxFontHeight; i++)
			{
                fonts[i] = new Font(device, (i < 1) ? 1 : i, 0, FontWeight.Regular, 0, false, CharacterSet.Default,
                                Precision.TtOnly, FontQuality.ClearType, PitchAndFamily.FixedPitch, Settings.Default.Font.FontFamily.Name);

                if (fonts[i] == null)
                    return false;
			}
            return true;
        }

        public void Render(float apptime)
        {
            if (device == null)
                return;

            // Clear the backbuffer to our backgrounds color 
            device.Clear(ClearFlags.Target, Settings.Default.BackgroundColor, 1.0f, 0);

            // Make device ready for drawing
            device.BeginScene();

            _layout = View.Default.Layout;

            if (_layout != null)
                DrawCodeview();

            // End drawing
            device.EndScene();

            // Flip backbuffers
            device.Present();
        }

        private void DrawCodeview()
        {
            int lastline = 0;

            for (int i = 0; i < _layout.lines.Length; i++)
            {
                LineLayout ll = _layout.lines[i];
                if (ll.shown)
                {
                    DrawLine(i);
                }
            }

            device.Clear(ClearFlags.Target,
                Settings.Default.BackgroundColor, 1.0f, 0, new System.Drawing.Rectangle[] { 
                    new System.Drawing.Rectangle(0,
                    0,
                    Settings.NumberingWidth,
                    this.Size.Height
                    
                )});

            for (int i = 0; i < _layout.lines.Length; i++)
            {
                LineLayout ll = _layout.lines[i];
                if (ll.shown)
                {
                    // TODO: NumberingWidth should be dynamic and be calculated based on font size and number of digits shown
                    DrawString(Settings.NumberingWidth - MeasureString((int)ll.height, "" + (i + 1)) - 8, (int)ll.y, (int)ll.height, "" + (i + 1), System.Drawing.Color.Black);
                    
                    if (i - lastline > 4 && Settings.Default.DrawSeperator)
                    {
                        switch (Settings.Default.SegmentSeperatorStyle)
                        {
                            case Seperators.SolidLine:
                                device.Clear(ClearFlags.Target,
                                    Settings.Default.SeperatorColor,
                                    1.0f,
                                    0,
                                    new System.Drawing.Rectangle[] { new System.Drawing.Rectangle(0, (int)ll.y, this.Width, 1)
                                        });
                                break;

                            case Seperators.LinenumberIndication:
                                device.Clear(ClearFlags.Target,
                                    Settings.Default.SeperatorColor,
                                    1.0f,
                                    0,
                                    new System.Drawing.Rectangle[] { new System.Drawing.Rectangle(0, (int)ll.y, NumberingWidth, 1) 
                                        });
                                break;

                            default:
                                break;
                        }
                    }
                    lastline = i;
                }

            }


            // Draw seperator between line numbers and code
            device.Clear(ClearFlags.Target,
                        Settings.Default.SeperatorColor,
                1.0f,
                0,
                new System.Drawing.Rectangle[] { 
                    new System.Drawing.Rectangle(Settings.NumberingWidth,
                    0,
                    1,
                    this.Height)
                });
        }

        private void MarkFoundText()
        {
            if (Model.Default.SearchResultColEnd != 0)
            {
                LineLayout ll = _layout.lines[Model.Default.SearchResultLine];

                string line = Model.Default.Document[Model.Default.SearchResultLine];


                string tLine = line.Substring(0, Model.Default.SearchResultColStart).TrimStart();
                if (tLine.Length != tLine.TrimEnd().Length)
                {
                    tLine = "".PadLeft(tLine.Length - tLine.TrimEnd().Length, ' ') + tLine;
                }

                int markStart = MeasureString((int)ll.height, tLine) + (int)ll.x + NumberingWidth - HScrollValue;


                //TODO: Adjust scrollbar
                /*if (markStart > HScrollValue + this.Width)
                    HScrollValue = markStart-NumberingWidth-50;
                else if(markStart < HScrollValue)
                    HScrollValue = (int)Math.Max(markStart - NumberingWidth - 50, 0f);*/

                tLine = line;

                int markWidth = MeasureString((int)ll.height, tLine.Substring(Model.Default.SearchResultColStart, Model.Default.SearchResultColEnd - Model.Default.SearchResultColStart));

                device.Clear(ClearFlags.Target,
                    Settings.Default.SelectedColor,
                    1.0f,
                    0,
                    new System.Drawing.Rectangle[] {
                    new System.Drawing.Rectangle(markStart,
                    (int)ll.y,
                    markWidth, (int)ll.height)
                });
            }
        }

        private void DrawLine(int line)
        {
            int whitespace = 0;

            LineLayout ll = _layout.lines[line];
            int x = NumberingWidth - HScrollValue + (int)ll.x;

            System.Drawing.Color backgroundColor = Settings.Default.FocusColor;

            if (Model.Default.SelectedLine == line)
                backgroundColor = Settings.Default.SelectedColor;
            else if (line == Model.Default.Focus.Center)
                backgroundColor = Settings.Default.FocusCenterColor;
            else
            {
                // Biased focus towards showing the focus color for a longer time.
                // This way the new focus area is faded in during the first half of the animation
                // and the old focus starts to fade out at the same time.
                float biasedFocus = Math.Min(ll.focus * 1.5f, 1f); 
                
                System.Drawing.Color semanticColor = ColorUtilities.Interpolate(Settings.Default.BackgroundColor, Settings.Default.SemanticColor, ll.doi.semantic);
                backgroundColor = ColorUtilities.Interpolate(semanticColor, Settings.Default.FocusColor, biasedFocus);
            }

            // fill backgroundcolor
            device.Clear(ClearFlags.Target,
                backgroundColor,
                1.0f,
                0,
                new System.Drawing.Rectangle[] { 
                    new System.Drawing.Rectangle(0,
                    (int)ll.y,
                    this.Width,
                    (int)ll.height + 1)
                });

            //Draw found text
            if(Model.Default.SearchResultLine == line)
                MarkFoundText();

            for (int i = 0; i < Model.Default.TokenizedDocument[line].Length; i++)
            {
                string text = Model.Default.TokenizedDocument[line][i].Value;

                if (i == 0)
                {
                    text = text.TrimStart();
                }

                // Add whitespace from end of last string to start of this string,
                // due to Measurestring trimming whitespace at the end of a string
                text = "".PadLeft(whitespace, ' ') + text;

                DrawString(x, (int)ll.y, (int)ll.height,
                    text,
                    Model.Default.TokenizedDocument[line][i].Color);

                x += MeasureString((int)ll.height, text); 

                whitespace = text.Length - text.TrimEnd().Length;
            }


        }

        // TODO: Fix font height problem, font should never exceed _maxFontHeight,
        // but either the layout is wrong or the DOI strategy returns too large weights 
        private int MeasureString(int height, string text)
        {
            Font f = fonts[Math.Min(height, _maxFontHeight - 1)];
            return f.MeasureString(null, text, DrawTextFormat.NoClip, 0).Width;
        }

        private void DrawString(int x, int y, int height, string text, System.Drawing.Color color)
        {
            Font f = fonts[Math.Min(height,_maxFontHeight - 1)];
            f.DrawText(null, text, new System.Drawing.Point(x,y), color);
        }

        private int MeasureDocument()
        {
            int height = (int)Settings.Default.Font.GetHeight();
            int maxWidth = 0;

            for (int i = 0; i < Model.Default.Document.Length; i++)
            {
                _scrollableWidth = Math.Max(MeasureString(height, Model.Default.Document[i]), _scrollableWidth);
            }

            return maxWidth;
        }

        private void OnDeviceLost(object sender, EventArgs e)
        {
            for (int i = 0; i < _maxFontHeight; i++)
			{
                if (fonts[i] != null)
                    fonts[i].OnLostDevice();
			}
            //MessageBox.Show("Lost!");
        }

        private void OnDeviceReset(object sender, EventArgs e)
        {
            for (int i = 0; i < _maxFontHeight; i++)
			{
                if (fonts[i] != null)
                    fonts[i].OnResetDevice();
			}
            //MessageBox.Show("Reset!");
        }

        void OnDeviceDisposing(object sender, EventArgs e)
        {
            for (int i = 0; i < _maxFontHeight; i++)
            {
                if (fonts[i] != null)
                {
                    fonts[i].Dispose();
                    fonts[i] = null;
                }
            }
            //MessageBox.Show("Disposing!");
        }

        void OnDeviceResizing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("Resizing!");
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
                this.Render((float)(DateTime.Now - startTime).TotalSeconds);
            
        }

        public bool InitializeGraphics()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
                return true;
            }
            catch (DirectXException)
            {
                return false;
            }
        }
    }
}
