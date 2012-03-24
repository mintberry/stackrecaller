using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class FinallyNode : StatementNode
	{
        public FinallyNode(Token relatedtoken)
            : base(relatedtoken)
        {
            finallyBlock = new BlockStatement(relatedtoken);
        }
		private BlockStatement finallyBlock;
		public BlockStatement FinallyBlock
		{
			get { return finallyBlock; }
			set { finallyBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("finally");
			this.NewLine(sb);
			finallyBlock.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFinallyStatement(this, data);
        }
	}
}
