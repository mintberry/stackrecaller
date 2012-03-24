using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class PPDefineNode : PPNode
	{
		public PPDefineNode(Token relatedToken) : base(relatedToken)
		{
		}
        public PPDefineNode(IdentifierExpression identifier)
            : base(identifier.RelatedToken)
		{
			this.identifier = identifier;
		}
		private IdentifierExpression identifier;
		public IdentifierExpression Identifier
		{
			get { return identifier; }
			set { identifier = value; }
		}

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("#define ");
            this.identifier.ToSource(sb);
            this.NewLine(sb);
        }
	}
}
