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
    [ContentType("text")]
    [TagType(typeof(TextMarkerTag))]
    internal class TextInvisTaggerProvider : IViewTaggerProvider
    {


        #region IViewTaggerProvider Members

        ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView textView, ITextBuffer buffer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
