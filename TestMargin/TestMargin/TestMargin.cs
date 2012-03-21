using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;
using EnvDTE80;

namespace TestMargin
{

    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestMargin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestMargin";
        private IWpfTextView _textView;
        private bool _isDisposed = false;

        private DTE2 _dte;
        private Document _doc;
        private VCFileCodeModel _vccm;

        private ovCode _overviewport = null;                                 //the wpf control to display the overview

        /// <summary>
        /// Creates a <see cref="TestMargin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public TestMargin(IWpfTextView textView, DTE2 dte)
        {
            _textView = textView;
            _dte = dte;

            this.Width = 100;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);

            _textView.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(_textView_LayoutChanged);

            _textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(Caret_PositionChanged);

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
            _overviewport = new ovCode(_textView);

            this.Children.Add(_overviewport);

        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            int linecount = _textView.TextSnapshot.LineCount;
            //throw new NotImplementedException();
            System.Diagnostics.Trace.WriteLine("CaretPostitionChanged:" + _textView.TextSnapshot.GetLineNumberFromPosition(_textView.Caret.Position.BufferPosition));

        }

        void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            //System.Diagnostics.Trace.WriteLine("###         FirstVisibleLine: " + _textView.TextSnapshot.GetLineNumberFromPosition(_textView.TextViewLines.FirstVisibleLine.Start));

            //_doc = _dte.ActiveDocument;
            
            try
            {
                _vccm = _dte.ActiveDocument.ProjectItem.FileCodeModel as VCFileCodeModel;
                System.Diagnostics.Trace.WriteLine("{{{");
                foreach (CodeElement ce in _vccm.Functions)
                {
                    System.Diagnostics.Trace.WriteLine("###         DTEDocument:" + ce.Kind + " : " + ce.StartPoint.Line);
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Trace.WriteLine("@@@         Exception:" + e1.ToString());
            }
            //throw new NotImplementedException();
            if (_overviewport != null)
            {
                _overviewport.UpdateOV();                         //update the overview accroding to the change of main text area
            }
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


    }
}
