using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

using System.Windows.Controls;
using System.Windows.Media;
using TestMargin.Utils;

namespace TestMargin.Adorn
{

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class FocusArea : IWpfTextViewCreationListener
    {
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
            new TestAdorn(textView);
        }
    }

    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    class TestAdorn
    {
        private Image _image;
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;

        int CentralLine { get; set; }
        System.Windows.Point TopLeft { get; set; }
        ITextViewLine CentralViewLine { get; set; }

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public TestAdorn(IWpfTextView view)
        {
            _view = view;


            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("FocusArea");

            _view.ViewportHeightChanged += delegate { this.onSizeChange(); };
            _view.ViewportWidthChanged += delegate { this.onSizeChange(); };
        }

        public void onSizeChange()
        {
            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments();

            CentralViewLine = EditorActor.GetCentralLine(_view, emuParser.central_offset);
            if (CentralViewLine == null)
            {
                System.Diagnostics.Trace.WriteLine("--------------------CENTRALLINE NOT AVAILABLE NOW");
                return;
            }

            double totalheight = (emuParser.central_offset * 2 - 1) * CentralViewLine.Height;
            ReGenImage(totalheight);

            //Place the image in the top right hand corner of the Viewport
            Canvas.SetLeft(_image, CentralViewLine.Left);
            Canvas.SetTop(_image, CentralViewLine.Top);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, _image, null);
        }

        public void ReGenImage(double theight)
        {
            Brush brush = new SolidColorBrush(Colors.LemonChiffon);
            brush.Freeze();
            Brush penBrush = new SolidColorBrush(Colors.Gold);
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

            _image = new Image();
            _image.Source = drawingImage;
        }
    }

}
