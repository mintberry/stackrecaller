using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public abstract class ExpressionNode : BaseNode
	{
        public ExpressionNode(Token relatedToken)
            : base(relatedToken)
        {
        }
    }
}
