using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class TypeOfExpression : PrimaryExpression
	{
        public TypeOfExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public TypeOfExpression(ExpressionNode expression)
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
			sb.Append("typeof(");
			expression.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypeOfExpression(this, data);
        }
	}
}
