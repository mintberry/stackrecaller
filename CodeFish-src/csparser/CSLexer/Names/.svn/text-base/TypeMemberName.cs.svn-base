using System;
using System.Collections.Generic;
using System.Text;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class TypeMemberName : IdentifierName
    {
        private Scope scope;

        /// <summary>
        /// Initializes a new instance of the TypeMemberName class.
        /// </summary>
        public TypeMemberName(string name, NameVisibilityRestriction visibility, Scope scope, Context context)
            : base(name, visibility, context)
        {
            this.scope = scope;
        }

        public Scope Scope
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.scope;
            }
        }
    }
}
