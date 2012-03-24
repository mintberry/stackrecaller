using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class SizeOfExpression : PrimaryExpression
	{
        public SizeOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public SizeOfExpression(ExpressionNode expression)
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
			sb.Append("sizeof(");
			expression.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitSizeOfExpression(this, data);
        }
	}
}
