using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace TestMargin
{
    
    #region TestMargin Factory
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor
    /// to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(TestMargin.MarginName)]
    [Order(Before = /*"Wpf Vertical Scrollbar"*/PredefinedMarginNames.VerticalScrollBar)] //Ensure that the margin occurs below the horizontal scrollbar
    [MarginContainer(PredefinedMarginNames.VerticalScrollBarContainer)] //Set the container to the bottom of the editor window
    //before RightControl in Right or before Scrollbar in Scrollbar Container
    [ContentType("C/C++")] //Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        [Import]
        internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }   //just a test

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService = null;

        [Import]
        internal SVsServiceProvider serviceProvider = null;
        
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            _contentTypeRegistryService = ContentTypeRegistryService;
            _textBufferFactoryService = TextBufferFactoryService;

            DTE2 dte = (DTE2)serviceProvider.GetService(typeof(DTE));

            //System.Diagnostics.Trace.WriteLine(":" + _textBufferFactoryService.TextContentType.ToString());
            _curTextBuf = _textBufferFactoryService.CreateTextBuffer("test", _textBufferFactoryService.PlaintextContentType);
            
            return new TestMargin(textViewHost.TextView, dte);
        }

        private IContentTypeRegistryService _contentTypeRegistryService;
        private ITextBufferFactoryService _textBufferFactoryService;
        private ITextBuffer _curTextBuf;
    }
    #endregion

    
}
