using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;

namespace TestMargin.Taggers
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType("C/C++")]
    class OutlnTaggerProvider : ITaggerProvider
    {
        [ImportMany]
        internal IEnumerable<IViewTaggerProvider> viewTaggerProviderCollection { get; set; }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
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
                return null;
            }

            //create a single tagger for each buffer.
            Func<ITagger<T>> sc = delegate() { return new OutlnTagger(buffer, _titp.GetThyTagger()) as ITagger<T>; };
            return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
            //return null;
        }
        private TextInvisTaggerProvider _titp;
    }
    class DefCollapseOLTag : IOutliningRegionTag 
    {
        public DefCollapseOLTag() {}

        public object CollapsedForm
        {
            get
            {
                return null;
            }
        }

        public object CollapsedHintForm
        {
            get
            {
                return null;
            }
        }

        public bool IsDefaultCollapsed
        {
            get { return true; }
        }

        public bool IsImplementation
        {
            get { return false; }
        }
    }
}
