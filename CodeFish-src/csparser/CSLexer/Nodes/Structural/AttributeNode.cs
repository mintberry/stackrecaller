using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class AttributeNode : BaseNode
    {
        public AttributeNode(Token relatedToken)
            : base(relatedToken)
        {
        }

		private Modifier modifiers;
		public Modifier Modifiers
		{
            [System.Diagnostics.DebuggerStepThrough]
			get { return modifiers; }
            [System.Diagnostics.DebuggerStepThrough]
            set { modifiers = value; }
		}

		private QualifiedIdentifierExpression name;
        public QualifiedIdentifierExpression Name
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return name; }
            [System.Diagnostics.DebuggerStepThrough]
            set { name = value; }
		}

		private NodeCollection<AttributeArgumentNode> arguments;
		public NodeCollection<AttributeArgumentNode> Arguments
		{
			get { if (arguments == null) arguments = new NodeCollection<AttributeArgumentNode>(); return arguments; }
		}

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("[");
			if(modifiers != Modifier.Empty)
			{
				this.TraceModifiers(this.Modifiers, sb);
				sb.Append(": ");
			}
			name.ToSource(sb);

			if (arguments != null)
			{
				sb.Append("(");
				string comma = "";
				for (int i = 0; i < arguments.Count; i++)
				{
					sb.Append(comma);
					arguments[i].ToSource(sb);
					comma = ", ";
				}
				sb.Append(")");
			}
			sb.Append("]");
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            throw new NotSupportedException();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAttribute(this, data);
        }

    }
}
