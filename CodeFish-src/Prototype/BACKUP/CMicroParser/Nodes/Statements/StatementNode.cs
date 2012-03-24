using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class StatementNode : BaseNode
    {
        public StatementNode(Token relatedToken)
            : base(relatedToken)
        {
        }

        public override void ToSource(StringBuilder sb)
        {
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStatementNode(this, data);
        }

	}
}
