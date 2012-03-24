using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class NullCoalescingExpression : ExpressionNode
	{
         public NullCoalescingExpression(Token relatedtoken)
            : base(relatedtoken)
        {
        }


        public NullCoalescingExpression(ExpressionNode left, ExpressionNode right)
            : base(left.RelatedToken)
		{
			this.left = left;
			this.right = right;
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
			left.ToSource(sb);
            sb.Append(" ?? ");
            right.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitNullCoalescingExpression(this, data);
        }

	}
}
