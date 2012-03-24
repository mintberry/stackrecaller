using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class PostIncrementExpression : PrimaryExpression
	{
        public PostIncrementExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public PostIncrementExpression(ExpressionNode expression)
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
			expression.ToSource(sb);
			sb.Append("++");
		}


        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPostIncrementExpression(this, data);
        }

	}
}
