using System;
using System.Collections.Generic;
using System.Text;
using DDW.Enums;

namespace DDW.Names
{
    public abstract class IdentifierName
    {
        private string[] fullyQualifiedName;
        private NameVisibilityRestriction visibility;
        private IdentifierName parent;

        /// <summary>
        /// Initializes a new instance of the Identifier class.
        /// </summary>
        protected IdentifierName(string name, NameVisibilityRestriction visibility, Context context)
        {
            string[] curContext = context.GetContext();

            this.fullyQualifiedName = new string[curContext.Length + 1];
            this.fullyQualifiedName[0] = name;
            Array.Copy(curContext, 0, this.fullyQualifiedName, 1, curContext.Length);

            this.visibility = visibility;
        }

        /// <summary>
        /// Fully qualified name, in reverse order.
        /// e.g.: DDW.Names.Identifier will be saved as {"Identifier", "Names", "DDW"}
        /// </summary>
        public string[] FullyQualifiedName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.fullyQualifiedName;
            }
            [System.Diagnostics.DebuggerStepThrough]
            protected internal set
            {
                this.fullyQualifiedName = value;
            }
        }

        /// <summary>
        /// Who is allowed to see this identifier?
        /// </summary>
        public NameVisibilityRestriction Visibility
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.visibility;
            }
        }

        public IdentifierName Parent
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.parent;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.parent = value;
            }
        }
    }
}
