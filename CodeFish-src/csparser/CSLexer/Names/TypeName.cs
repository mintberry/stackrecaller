using System;
using System.Collections.Generic;
using System.Text;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class TypeName : IdentifierName
    {
        private string[] genericParameters;

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        protected TypeName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Context context)
            : base(name, visibility, context)
        {
            this.genericParameters = genericParameters;
        }

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        protected TypeName(string name, NameVisibilityRestriction visibility, Context context)
            : base(name, visibility, context)
        {
            this.genericParameters = new string[0];
        }

        public string[] GenericParameters
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.genericParameters;
            }
        }
    }
}
