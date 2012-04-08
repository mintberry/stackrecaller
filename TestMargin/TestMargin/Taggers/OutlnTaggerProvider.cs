﻿using System;
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
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
        //create a single tagger for each buffer.
        Func<ITagger<T>> sc = delegate() { return new OutlnTagger(buffer) as ITagger<T>; };
        //return buffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(sc);
        return null;
        } 
    }
}