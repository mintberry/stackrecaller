using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ConditionalExpression : ExpressionNode
	{
		public ConditionalExpression(Token relatedtoken) : base (relatedtoken)
		{
		}

        public ConditionalExpression(ExpressionNode test, ExpressionNode left, ExpressionNode right)
            : base(test.RelatedToken)
		{
			this.test = test;
			this.left = left;
			this.right = right;
		}


		private ExpressionNode test;
        public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

		protected ExpressionNode left;
		public ExpressionNode Left
		{
			get { return left; }
			set { left = value; }
		}

		protected ExpressionNode right;
		public ExpressionNode Right
		{
			get { return right; }
			set { right = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			test.ToSource(sb);
			sb.Append(" ? ");
			left.ToSource(sb);
			sb.Append(" : ");
			right.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConditionalExpression(this, data);
        }

	}
}
