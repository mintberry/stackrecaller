using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ConstantExpression : BaseNode
	{
		private ExpressionNode val;
		public ExpressionNode Value
		{
			get { return val; }
			set { val = value; }
        }


        public ConstantExpression(Token relatedtoken)
            : base(relatedtoken)
        {
        }

        public ConstantExpression(ExpressionNode val)
            : base(val.RelatedToken)
        {
            this.val = val;
        }

        public override void ToSource(StringBuilder sb)
        {
			if (val != null)
			{
				val.ToSource(sb);
			}
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstantExpressions(this, data);
        }
	
	}
}
