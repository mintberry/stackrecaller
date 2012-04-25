using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace TestMargin.Taggers
{
    class TextInvisTag : ClassificationTag, IOutliningRegionTag
    {
        

        public TextInvisTag(IClassificationType type)
            : base(type)
        {
            //System.Diagnostics.Trace.WriteLine("&&&                TIT: ");
        }
        //"ClassificationFormatDefinition/InvisFormat"
        //"invisclass.invis"

        object IOutliningRegionTag.CollapsedForm
        {
            get { return null; }
        }

        object IOutliningRegionTag.CollapsedHintForm
        {
            get { return null; }
        }

        bool IOutliningRegionTag.IsDefaultCollapsed
        {
            get { return false; }
        }

        bool IOutliningRegionTag.IsImplementation
        {
            get { return false; }
        }
    }
}
