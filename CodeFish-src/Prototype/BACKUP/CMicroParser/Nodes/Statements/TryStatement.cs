using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class TryStatement : StatementNode
	{
        public TryStatement(Token relatedToken)
            : base(relatedToken)
        {
            tryBlock = new BlockStatement(RelatedToken);
        }
        private BlockStatement tryBlock;
		public BlockStatement TryBlock
		{
			get { return tryBlock; }
			set { tryBlock = value; }
		}
		private NodeCollection<CatchNode> catchBlocks = new NodeCollection<CatchNode>();
		public NodeCollection<CatchNode> CatchBlocks
		{
			get { return catchBlocks; }
			set { catchBlocks = value; }
		}
		private FinallyNode finallyBlock;
		public FinallyNode FinallyBlock
		{
			get { return finallyBlock; }
			set { finallyBlock = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("try");
			this.NewLine(sb);
			tryBlock.ToSource(sb);
			foreach (CatchNode cb in catchBlocks)
			{				
				cb.ToSource(sb);
			}
			
			if (finallyBlock != null)
			{
				finallyBlock.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTryStatement(this, data);
        }
	}
}
