using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class OutNode : ExpressionNode
	{
        public OutNode(Token relatedToken)
            : base(relatedToken)
        {
        }

        public OutNode(ExpressionNode variableReference) : base(variableReference.RelatedToken)
		{
			this.variableReference = variableReference;
		}

		private ExpressionNode variableReference;
		public ExpressionNode VariableReference
		{
			get { return variableReference; }
			set { variableReference = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("out ");
			variableReference.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAddressOutNode(this, data);
        }

	}
}
