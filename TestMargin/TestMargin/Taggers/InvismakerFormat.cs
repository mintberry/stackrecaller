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
    [Export(typeof(EditorFormatDefinition))]
    [Name("ClassificationFormatDefinition/InvisFormat")]
    [UserVisible(true)]                                           //try invisible later
    internal class InvisMakerFormat : ClassificationFormatDefinition  //used to be markerformat
    {
        public InvisMakerFormat()
        {
            
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            
            this.DisplayName = "Invisible Lines";
            //this.ZOrder = 5;
        }
    }
}
