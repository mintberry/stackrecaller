using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ExpressionStatement : StatementNode
	{
        public ExpressionStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }

        public ExpressionStatement(ExpressionNode expression) :base(expression.RelatedToken)
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
			sb.Append(";");
			this.NewLine(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitExpressionStatement(this, data);
        }

	}
}
