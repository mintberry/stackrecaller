using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public abstract class PrimaryExpression : ExpressionNode, IMemberAccessible
	{
        public PrimaryExpression(Token relatedToken)
            : base(relatedToken)
        {
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPrimaryExpression(this, data);
        }
	}
}
