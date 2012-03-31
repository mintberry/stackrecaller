using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;


namespace TestMargin.Taggers
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("C/C++")]
    [TagType(typeof(TextMarkerTag))]
    internal class TextInvisTaggerProvider : IViewTaggerProvider
    {
        //[Import]
        //internal ITextSearchService TextSearchService { get; set; }

        //[Import]
        //internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }


        #region IViewTaggerProvider Members

        ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView textView, ITextBuffer buffer)
        {
            //throw new NotImplementedException();
            if (textView.TextBuffer != buffer)
                return null;
            return new TextInvisTagger(textView, buffer) as ITagger<T>;
        }

        #endregion
    }
}
