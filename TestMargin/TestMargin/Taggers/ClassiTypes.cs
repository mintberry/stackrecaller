using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestMargin.Taggers
{
    static class ClassiTypes
    {
        [Export]
        [Name("invisclass")]
        internal static ClassificationTypeDefinition InvisClassDefinition = null;

        [Export]
        [Name("invisclass.invis")]
        [BaseDefinition("invisclass")]
        internal static ClassificationTypeDefinition InvisClassInvisDefinition = null;

        [Export]
        [Name("invisclass.careton")]
        [BaseDefinition("invisclass")]
        internal static ClassificationTypeDefinition InvisClassCaretOnDefinition = null;

        [Export]
        [Name("invisclass.central")]
        [BaseDefinition("invisclass")]
        internal static ClassificationTypeDefinition InvisClassCentralDefinition = null;
    }
}
