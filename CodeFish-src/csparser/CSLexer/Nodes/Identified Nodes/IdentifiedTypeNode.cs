using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    public class IdentifiedLocalTypeNode : IdentifiedTypeNode
    {
        private string fullyQualifiedName;

        /// <summary>
        /// Initializes a new instance of the IdentifiedLocalTypeNode class.
        /// </summary>

        public IdentifiedLocalTypeNode(string fullyQualifiedName, Token relatedToken): base(relatedToken)
        {
            this.fullyQualifiedName = fullyQualifiedName;
        }

        public string FullyQualifiedName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.fullyQualifiedName;
            }
        }
    }

    public class IdentifiedExternalTypeNode : IdentifiedTypeNode
    {
        private Type type;

        /// <summary>
        /// Initializes a new instance of the IdentifiedExternalTypeNode class.
        /// </summary>
        public IdentifiedExternalTypeNode(Type type, Token relatedToken): base(relatedToken)
        {
            this.type = type;
        }

        public Type Type
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.type;
            }
        }
    }

    public abstract class IdentifiedTypeNode : TypeNode
    {
        public IdentifiedTypeNode(Token relatedToken)
            : base(relatedToken)
        {
        }
    }
}
