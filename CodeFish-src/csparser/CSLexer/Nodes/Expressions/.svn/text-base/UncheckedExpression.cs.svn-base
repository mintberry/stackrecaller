using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class UncheckedExpression : PrimaryExpression
	{
        public UncheckedExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public UncheckedExpression(ExpressionNode expression)
            : base(expression.RelatedToken)
		{
			this.expression = expression;
		}

		private ExpressionNode expression;
		public ExpressionNode Expression
		{
			get { return expression; }
			set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("unchecked(");
			expression.ToSource(sb);
			sb.Append(")");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUncheckedExpression(this, data);
        }

	}
}
