using System;
using System.Collections.Generic;
using System.Text;
using DDW.Enums;

namespace DDW.Names
{
    public class MethodName : TypeMemberName
    {
        private string[] genericParameters;

        /// <summary>
        /// Initializes a new instance of the MethodName class.
        /// </summary>
        public MethodName(string name, NameVisibilityRestriction visibility, string[] genericParameters, Scope scope, Context context)
            : base(name, visibility, scope, context)
        {
            this.genericParameters = genericParameters;
        }

        /// <summary>
        /// Initializes a new instance of the TypeName class.
        /// </summary>
        public MethodName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, scope, context)
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
