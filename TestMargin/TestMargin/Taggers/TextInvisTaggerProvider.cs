﻿using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Outlining;
using TestMargin.OverViews;


namespace TestMargin.Taggers
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("C/C++")]
    [TagType(typeof(ClassificationTag))]
    class TextInvisTaggerProvider : IViewTaggerProvider
    {
        //[Import]
        //internal ITextSearchService TextSearchService { get; set; }

        //[Import]
        //internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        [Import]
        internal IClassificationTypeRegistryService registry { set; get; }

        //[Import]
        //internal IOutliningManagerService outlinemgrservice { get; set; }

        TextInvisTagger _val;

        #region IViewTaggerProvider Members

        ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView textView, ITextBuffer buffer)
        {
            //throw new NotImplementedException();
            if (textView.TextBuffer != buffer)
                return null;
            //IOutliningManager om = outlinemgrservice.GetOutliningManager(textView);
            _val = new TextInvisTagger(textView, buffer, registry);
            return _val as ITagger<T>;
        }

        #endregion

        public TextInvisTagger GetThyTagger()
        {
            System.Diagnostics.Trace.WriteLine("~~~               GetThyTagger" + _val);
            return _val;
        }
    }
}
