using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public abstract class LiteralNode : PrimaryExpression
	{
        public LiteralNode(Token relatedToken)
            : base(relatedToken)
        {
        }
	}
}
