using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class PPEndIfNode : PPNode
	{
        public PPEndIfNode(Token relatedToken)
            : base(relatedToken)
		{
		}

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("#endif");
            this.NewLine(sb);
        }
	}
}
