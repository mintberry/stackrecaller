//nf4Ec2yx2kg4 
//https:// QiXiaochen@code.google.com/p/stack-recaller/
//https:// littne@bitbucket.org/littne/stackrecaller
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.ComponentModelHost;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using TestMargin.Taggers;

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
        //[Import]
        //internal IContentTypeRegistryService ContentTypeRegistryService { get; set; }   //just a test

        //[Import]
        //internal ITextBufferFactoryService TextBufferFactoryService = null;

        //[Import]
        //internal SVsServiceProvider serviceProvider = null;

        // consume the tagger provider
        [ImportMany]
        internal IEnumerable<IViewTaggerProvider> viewTaggerProviderCollection { get; set; }


        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            int iErrStat;

            //_contentTypeRegistryService = ContentTypeRegistryService;
            //_textBufferFactoryService = TextBufferFactoryService;

            //DTE2 dte = (DTE2)serviceProvider.GetService(typeof(DTE));

            //iErrStat = GetHiddenTextManager(serviceProvider);


            //IComponentModel componentModel = (IComponentModel)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));
            //IVsEditorAdaptersFactoryService adapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            
            ////System.Diagnostics.Trace.WriteLine(":" + _textBufferFactoryService.TextContentType.ToString());
            //_curTextBuf = _textBufferFactoryService.CreateTextBuffer("test", _textBufferFactoryService.PlaintextContentType);

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
                return null;
            }
            return new TestMargin(textViewHost.TextView, _titp.GetThyTagger());
        }


        private int GetHiddenTextManager(System.IServiceProvider isp)
        {
            //int iErrStat;
            System.Guid SID = typeof(SVsTextManager).GUID;
            System.Guid IID = typeof(IVsHiddenTextManager).GUID;
            //System.IntPtr ipout;

            // iErrStat = isp.QueryService(SID, IID, out ipout);
            // _htm = (IVsHiddenTextManager)Marshal.GetObjectForIUnknown(ipout);

            _htm = isp.GetService(typeof(SVsTextManager)) as IVsHiddenTextManager;
            if (_htm != null)
            {
                System.Diagnostics.Trace.WriteLine("&&&                INIT: ");
            }
            return 0;
        }
        private IContentTypeRegistryService _contentTypeRegistryService;
        private ITextBufferFactoryService _textBufferFactoryService;
        private ITextBuffer _curTextBuf;
        private IVsHiddenTextManager _htm;

        private TextInvisTaggerProvider _titp;
    
    }
    #endregion

}
