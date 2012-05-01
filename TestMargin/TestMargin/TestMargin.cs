using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.VCCodeModel;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using EnvDTE;
using EnvDTE80;
using TestMargin.OverViews;
using TestMargin.Taggers;

namespace TestMargin
{

    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestMargin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestMargin";
        public IWpfTextView _textView { get; set; }
        private bool _isDisposed = false;

        private DTE2 _dte;
        private Document _doc;
        private VCFileCodeModel _vccm;

        private IVsHiddenTextManager _htm;
        private IVsEditorAdaptersFactoryService _afService;

        //private List<OvLine> _ovlc;
        private OvCollection _ovc;
        private J4I j4i;
        private TextInvisTaggerProvider _tit_provider;

        private ovCode _overviewport = null;                                 //the wpf control to display the overview


        public TestMargin(IWpfTextView textView)
        {
            _textView = textView;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);

            this.j4i = new J4I();

            _ovc = new OvCollection(this);

            _textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_textView_LayoutChanged);

            _textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(Caret_PositionChanged);

            this.MouseMove += new System.Windows.Input.MouseEventHandler(TestMargin_MouseMove);

            this._textView.ViewportHeightChanged += new EventHandler(_textView_ViewportHeightChanged);

            this._textView.ViewportWidthChanged += new EventHandler(_textView_ViewportWidthChanged);
        }

        /// <summary>
        /// Creates a <see cref="TestMargin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public TestMargin(IWpfTextView textView, DTE2 dte, IVsHiddenTextManager htm, IVsEditorAdaptersFactoryService afService)
        {
            _textView = textView;
            _dte = dte;
            _htm = htm;
            _afService = afService;
           

            //this.Width = _textView.ViewportWidth / 4;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);

            this.j4i = new J4I();

            _ovc = new OvCollection(this);

            _textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_textView_LayoutChanged);

            _textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(Caret_PositionChanged);

            this.MouseMove += new System.Windows.Input.MouseEventHandler(TestMargin_MouseMove);

            this._textView.ViewportHeightChanged += new EventHandler(_textView_ViewportHeightChanged);

            this._textView.ViewportWidthChanged += new EventHandler(_textView_ViewportWidthChanged);

            //Document d = _dte.ActiveDocument;

            //try
            //{
            //    VCCodeModel cm = (VCCodeModel)_dte.Solution.Item(1);
            //    System.Diagnostics.Trace.WriteLine("###         DTEDocument:" + cm.Language);  //always fail with dte
            //}
            //catch (Exception e)
            //{
            //    System.Diagnostics.Trace.WriteLine("@@@         Exception:" + e.ToString());
            //}


            // Add a green colored label that says "Hello World!"
            //Label label = new Label();
            //label.Background = new SolidColorBrush(Colors.LightGreen);
            //label.Content = "Hello world!";
            //this.Children.Add(label);
            
            //_overviewport = new ovCode(_textView);

            //this.Children.Add(_overviewport);

        }

        void _textView_ViewportWidthChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.Width = _textView.ViewportWidth / 5;
            this.Height = _textView.ViewportHeight;

            _ovc.IsRedraw = true;
            _ovc.ReGenOv();
        }

        void _textView_ViewportHeightChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.Width = _textView.ViewportWidth / 5;
            this.Height = _textView.ViewportHeight;

            _ovc.IsRedraw = true;
            _ovc.ReGenOv();
        }

        /// <summary>
        /// handle mouse move on overview, sync with editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TestMargin_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //throw new NotImplementedException();
            System.Windows.Point hoverpoint = e.GetPosition(this);

        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            int linecount = _textView.TextSnapshot.LineCount;
            //throw new NotImplementedException();
            //System.Diagnostics.Trace.WriteLine("CaretPostitionChanged:" + _textView.TextSnapshot.GetLineNumberFromPosition(_textView.Caret.Position.BufferPosition));
            ITextSnapshotLine ssl = e.NewPosition.BufferPosition.GetContainingLine();
            
        }

        void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if(e.VerticalTranslation != true)
            {
                this.Width = _textView.ViewportWidth / 5;
                this.Height = _textView.ViewportHeight;

                //_ovc.ReGenOv();
            }

            if(_htm != null)
            {
                int iErrStat;
                IVsHiddenTextSession hts = null;
                iErrStat = _htm.GetHiddenTextSession(_afService.GetBufferAdapter(_textView.TextBuffer), out hts);
                //System.Diagnostics.Trace.WriteLine("&&&                TEXTMGR: " + _htm.ToString());
                if (iErrStat == VSConstants.S_OK)
                {
                    System.Diagnostics.Trace.WriteLine("&&&                HIDMGR: ");
                }
                else 
                {
                    iErrStat = _htm.CreateHiddenTextSession(0, _textView.TextBuffer, null, out hts);
                    if (iErrStat == VSConstants.S_OK)
                    {
                        System.Diagnostics.Trace.WriteLine("&&&                HIDSES: ");
                    }
                }
            }

            //System.Diagnostics.Trace.WriteLine("###         margin:" + this.Height);
            //System.Diagnostics.Trace.WriteLine("###         FirstVisibleLine: " + _textView.TextSnapshot.GetLineNumberFromPosition(_textView.TextViewLines.FirstVisibleLine.Start));

            //_doc = _dte.ActiveDocument;
            
            //try
            //{
            //    _vccm = _dte.ActiveDocument.ProjectItem.FileCodeModel as VCFileCodeModel;
            //    System.Diagnostics.Trace.WriteLine("{{{");
            //    foreach (CodeElement ce in _vccm.Functions)
            //    {
            //        System.Diagnostics.Trace.WriteLine("###         DTEDocument:" + ce.Kind + " : " + ce.StartPoint.Line);
            //    }
            //}
            //catch (Exception e1)
            //{
            //    System.Diagnostics.Trace.WriteLine("@@@         Exception:" + e1.ToString());
            //}
            ////throw new NotImplementedException();
            //if (_overviewport != null)
            //{
            //    _overviewport.UpdateOV();                         //update the overview accroding to the change of main text area
            //}
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(MarginName);
        }

        

        #region IWpfTextViewMargin Members

        /// <summary>
        /// The <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation
        /// of the margin.
        /// </summary>
        public System.Windows.FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin Members

        public double MarginSize
        {
            // Since this is a horizontal margin, its width will be bound to the width of the text view.
            // Therefore, its size is its height.
            get
            {
                ThrowIfDisposed();
                return this.ActualHeight;
            }
        }

        public bool Enabled
        {
            // The margin should always be enabled
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        /// <summary>
        /// Returns an instance of the margin if this is the margin that has been requested.
        /// </summary>
        /// <param name="marginName">The name of the margin requested</param>
        /// <returns>An instance of TestMargin or null</returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == TestMargin.MarginName) ? (IWpfTextViewMargin)this : null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
        #endregion


        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            
        }

        


        #region Helpers
        public static float WidthPerChar(ITextView textView) 
        {
            string s = textView.TextViewLines.FirstVisibleLine.Extent.GetText();                          //maybe should not use first visible line, use mid-line
            int iTabCount = Regex.Matches(s, @"\t").Count;
            int iCharCount = iTabCount * 4 + s.Length - iTabCount;
            float widthpch = (float)(textView.TextViewLines.FirstVisibleLine.TextWidth / iCharCount);
            //System.Diagnostics.Trace.WriteLine("###         WIDTH:" + iCharCount + " : " + _textView.TextViewLines.FirstVisibleLine.TextWidth);
            return widthpch;
        }

        private int VLine2SLine(ITextViewLine tvl) 
        {
            //to be implemented, return snapshotline number
            return 0;
        }

        static public int GetViewLineNumber(ITextViewLine tvl)
        {
            return tvl.Snapshot.GetLineNumberFromPosition(tvl.Start);
        }

        private int GetLineNumberFromPoint(System.Windows.Point point) 
        {

            return 0;
        }
        #endregion  
    }

    /// <summary>
    /// class that just for import
    /// </summary>
    class J4I 
    {
        [Import(typeof(IViewTaggerProvider))]
        public IViewTaggerProvider vt_provider { get; set; }

        private CompositionContainer _container;

        public J4I() 
        {
            return;//not using this class
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(TestMargin).Assembly));
            Assembly x = Assembly.GetEntryAssembly();
            catalog.Catalogs.Add(new AssemblyCatalog(x));
            //catalog.Catalogs.Add(new TypeCatalog(typeof(IClassificationTypeRegistryService)));
            //catalog.Catalogs.Add(new AssemblyCatalog(@"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"));
            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);
            //Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);

            }
            catch (CompositionException compositionException)
            {
                System.Diagnostics.Trace.WriteLine("!!!               " + compositionException.Message);
            }
            
        }
    }
}
