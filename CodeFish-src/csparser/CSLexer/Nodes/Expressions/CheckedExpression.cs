using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class CheckedExpression : PrimaryExpression
	{
        public CheckedExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public CheckedExpression(ExpressionNode expression)
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
			sb.Append("checked(");
			expression.ToSource(sb);
			sb.Append(")");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCheckedExpression(this, data);
        }

	}
}
