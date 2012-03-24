using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public abstract class PPNode : ExpressionNode
	{
        public PPNode(Token relatedToken)
            : base(relatedToken)
        {
        }
	}
}
