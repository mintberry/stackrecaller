using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class ElementAccessExpression : PrimaryExpression
	{
        public ElementAccessExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public ElementAccessExpression(PrimaryExpression leftSide)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
		}
        public ElementAccessExpression(PrimaryExpression leftSide, ExpressionList expressions)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
			this.expressions = expressions;
		}


		private PrimaryExpression leftSide;
		public PrimaryExpression LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; }
		}

		private ExpressionList expressions;
		public ExpressionList Expressions
		{
			get { return expressions; }
			set { expressions = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			leftSide.ToSource(sb);

			sb.Append("[");
			expressions.ToSource(sb);
			sb.Append("]");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitElementAccessExpression(this, data);
        }

	}
}
