﻿using System;
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
    //maker for caret on
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "invisclass.careton")]
    [Name("ClassificationFormatDefinition/InvisFormat")]
    [UserVisible(true)]                                           //try invisible later
    [Order(After = Priority.High)]
    internal sealed class InvisMakerFormat : ClassificationFormatDefinition  //used to be markerformat
    {
        public InvisMakerFormat()
        {
            
            this.BackgroundColor = Colors.DarkViolet;
            //this.ForegroundColor = Colors.Crimson;
            
            //this.FontTypeface = new Typeface("Courier New");
            this.DisplayName = "Invisible Lines";
            //this.ZOrder = 5;
        }
    }

    //maker for invisible lines
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "invisclass.invis")]
    [Name("ClassificationFormatDefinition/InvisClassInvis")]
    [UserVisible(true)]                                           //try invisible later
    [Order(After = Priority.High)]
    internal sealed class InvisClassFormat : ClassificationFormatDefinition  //used to be markerformat
    {
        public InvisClassFormat()
        {

            //this.BackgroundColor = Colors.Crimson;
            this.ForegroundColor = Colors.Crimson;
            this.FontRenderingSize = 10;
            this.FontTypeface = new Typeface("Courier New");
            this.DisplayName = "Invisible Lines";
            //this.ZOrder = 5;
        }
    }
}
