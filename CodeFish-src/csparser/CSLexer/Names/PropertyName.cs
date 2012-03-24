using System;
using System.Collections.Generic;
using System.Text;
using DDW.Enums;

namespace DDW.Names
{
    public class PropertyName : TypeMemberName
    {
        private PropertyAccessors accessors;
        private NameVisibilityRestriction setVisibility;

        /// <summary>
        /// Initializes a new instance of the PropertyName class.
        /// </summary>
        public PropertyName(string name, 
            NameVisibilityRestriction getVisibility, NameVisibilityRestriction setVisibility, 
            Scope scope, PropertyAccessors accessors, Context context)
            : base(name, getVisibility, scope, context)
        {
            this.accessors = accessors;
            this.setVisibility = setVisibility;
        }

        public PropertyAccessors Accessors
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.accessors;
            }
        }

        public NameVisibilityRestriction SetVisibility
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.setVisibility;
            }
        }
   }
}
