using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class AddressOfExpression : PrimaryExpression
	{
        public AddressOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public AddressOfExpression(ExpressionNode expression) : base(expression.RelatedToken)
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
			sb.Append("&");
            expression.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAddressOfExpression(this, data);
        }

	}
}
