using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class UnaryCastExpression : UnaryExpression
	{
        public UnaryCastExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public UnaryCastExpression(IType type, ExpressionNode child, Token relatedToken)
            : base(relatedToken)
		{
			this.type = type;
			this.child = child;
		}
		private IType type;
		public IType Type
		{
			get { return type; }
			set { type = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("(");
			type.ToSource(sb);
			sb.Append(")");
			child.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUnaryExpression(this, data);
        }


	}
}

