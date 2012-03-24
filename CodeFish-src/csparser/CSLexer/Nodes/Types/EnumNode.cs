using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    public class EnumNode : ConstructedTypeNode
	{
        public EnumNode(Token relatedToken)
            : base(relatedToken)
        {
        }
		private IType baseClass;
        public IType BaseClass
		{
			get { return baseClass; }
			set { baseClass = value; }
		}

        private object val = null;
        public object Value
		{
			get { return val; }
			set { val = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			//todo: enumNode to source

            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
            }

            if (Value is DDW.Collections.NodeCollection<EnumNode>)
            {
                TraceModifiers(this.Modifiers, sb);

                sb.Append("enum ");

                this.Name.ToSource(sb);

                sb.Append("{ ");

                DDW.Collections.NodeCollection<EnumNode> coll = (DDW.Collections.NodeCollection<EnumNode>)Value;

                foreach (EnumNode expr in coll)
                {
                    expr.ToSource(sb);
                    sb.Append(", ");
                }

                sb.Remove(sb.Length - 2, 2);

                sb.Append("}; ");
                sb.Append(Environment.NewLine);
            }
            else
            {
                Name.ToSource(sb);
                if (Value != null)
                {
                    sb.Append("= ");
                    ((ExpressionNode)Value).ToSource(sb);
                }
            }
			// todo: enum members can have attributes
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitEnumDeclaration(this, data);
        }
	}
}
