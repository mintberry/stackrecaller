using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;

using System.Windows.Controls;
using System.Windows.Media;
using TestMargin.Utils;
using TestMargin.Taggers;

namespace TestMargin.Adorn
{

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("C/C++")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class FocusArea : IWpfTextViewCreationListener
    {
        [ImportMany]
        internal IEnumerable<IViewTaggerProvider> viewTaggerProviderCollection { get; set; }

        [Import]
        internal IViewTagAggregatorFactoryService tagaggregatorservice = null;
        /// <summary>
        /// Defines the adornment layer for the scarlet adornment. This layer is ordered 
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("FocusArea")]
        [Order(Before = PredefinedAdornmentLayers.Text)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        /// <summary>
        /// Instantiates a TestAdorn manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            foreach (IViewTaggerProvider vtp in viewTaggerProviderCollection)
            {
                if (vtp is TextInvisTaggerProvider)
                {
                    _titp = vtp as TextInvisTaggerProvider;
                    break;
                }
            }
            if (_titp == null)
            {
                System.Diagnostics.Trace.WriteLine("no valid tagger, exit");
                return;
            }
            new TestAdorn(textView, _titp.GetThyTagger(), tagaggregatorservice);
        }
        private TextInvisTaggerProvider _titp;
    }

    /// <summary>
    /// Adornment class that draws a focus area of the viewport
    /// </summary>
    class TestAdorn
    {
        private Image _image;
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;
        private TextInvisTagger _tit;

        int CentralLine { get; set; }
        System.Windows.Point TopLeft { get; set; }
        ITextViewLine TopViewLine { get; set; }
        ITagAggregator<TextInvisTag> tagAggregator { get; set; }

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public TestAdorn(IWpfTextView view, TextInvisTagger tit, IViewTagAggregatorFactoryService tafs)
        {
            _view = view;
            _tit = tit;

            tagAggregator = tafs.CreateTagAggregator<TextInvisTag>(_view);

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("FocusArea");

            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };

            //_tit.ScrollNumberFixed += delegate { this.onSizeChange(); };
            _view.LayoutChanged += delegate { this.onSizeChange(); };            //may have problems
            //_view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_view_LayoutChanged);
        }

        void _view_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            SnapshotSpan focusarea = _tit.GetSpan4FocusArea();
            double height = TopViewLine.Height;
            //throw new NotImplementedException();
            foreach (ITextViewLine itv in e.NewOrReformattedLines)
            {
                foreach (TextInvisTag tiTag in tagAggregator.GetTags(itv.ExtentAsMappingSpan))
                {
                    if (tiTag.ClassificationType == _tit.GetICT4FocusArea("invisclass.lower.focus"))
                    {
                        ReGenImage(height);
                        _adornmentLayer.AddAdornment(new SnapshotSpan(itv.Start, itv.End), tiTag, _image);
                    }
                }
            }
        }

        public void onSizeChange()
        {
            emuParser.ReCalFocusAreaHeight(this._view);
            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments();

            TopViewLine = EditorActor.GetTopLine(_view, _tit.GetCentralLine4Ov(), emuParser.central_offset);
            if (TopViewLine == null)
            {
                //System.Diagnostics.Trace.WriteLine("--------------------CENTRALLINE NOT AVAILABLE NOW");
                return;
            }

            double totalheight = (emuParser.central_offset * 2 - 1) * TopViewLine.Height;
            ReGenImage(totalheight);

            //Place the image in the top right hand corner of the Viewport
            Canvas.SetLeft(_image, _view.ViewportLeft);
            Canvas.SetTop(_image, TopViewLine.Top);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _image, null);
            _tit.TriggerBezier();
        }

        public void ReGenImage(double theight)
        {
            Brush brush = new SolidColorBrush(Colors.LemonChiffon);
            brush.Freeze();
            Brush penBrush = new SolidColorBrush(Colors.White);               //no paddings
            penBrush.Freeze();
            Pen pen = new Pen(penBrush, 0.5);
            pen.Freeze();

            //draw a square with the created brush and pen, specify the start point and width, length of the rect
            System.Windows.Rect r = new System.Windows.Rect(0, 0, _view.ViewportWidth, theight);
            Geometry g = new RectangleGeometry(r);
            GeometryDrawing drawing = new GeometryDrawing(brush, pen, g);
            drawing.Freeze();

            DrawingImage drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            _image = new Image();//
            _image.Source = drawingImage;
        }
    }

}
