using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class BaseAccessExpression : PrimaryExpression
	{
        ExpressionNode expression = null;
        public ExpressionNode Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
            }
        }

		public BaseAccessExpression(Token relatedToken) :base(relatedToken)
		{
		}

        public BaseAccessExpression(ExpressionNode expression) :base(expression.RelatedToken)
        {
            this.expression = expression;
        }

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("base");
			if (expression != null)
			{
				sb.Append(".");
				expression.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBaseReferenceExpression(this, data);
        }

	}
}
