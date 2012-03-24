using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class InvocationExpression : PrimaryExpression
	{
        public InvocationExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public InvocationExpression(PrimaryExpression leftSide, ExpressionList argumentList)
            : base(leftSide.RelatedToken)
		{
			this.leftSide = leftSide;
			this.argumentList = argumentList;
		}

		private ExpressionNode leftSide;
		public ExpressionNode LeftSide
		{
			get { return leftSide; }
			set { leftSide = value; }
		}

		private ExpressionList argumentList;
		public ExpressionList ArgumentList
		{
			get { return argumentList; }
			set { argumentList = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			leftSide.ToSource(sb);
			sb.Append("(");
			argumentList.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInvocationExpression(this, data);
        }

	}
}
