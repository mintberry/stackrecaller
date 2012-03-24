using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class PPIfNode : PPNode
	{
        public PPIfNode(Token relatedToken)
            : base(relatedToken)
		{
		}
        public PPIfNode(ExpressionNode expression)
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
            sb.Append("#if ");
            this.expression.ToSource(sb);
            this.NewLine(sb);
        }
	}
}
