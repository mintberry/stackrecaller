using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class CaseNode : StatementNode
	{
        public CaseNode(Token relatedToken)
            : base(relatedToken)
        {
            statementBlock = new BlockStatement(RelatedToken);
        }
		private bool isDefaultCase = false;
		public bool IsDefaultCase
		{
			get { return isDefaultCase; }
			set { isDefaultCase = value; }
		}
		private NodeCollection<ExpressionNode> ranges = new NodeCollection<ExpressionNode>();
		public NodeCollection<ExpressionNode> Ranges
		{
			get { return ranges; }
			set { ranges = value; }
		}

        private BlockStatement statementBlock;
		public BlockStatement StatementBlock
		{
			get { return statementBlock; }
			set { statementBlock = value; }
        }

		public override void ToSource(StringBuilder sb)
        {
			this.NewLine(sb);
			for (int i = 0; i < ranges.Count; i++)
			{
				sb.Append("case ");
				ranges[i].ToSource(sb);
				sb.Append(":");
				// some ugly special casing due to case blocks having multiple stmts without braces
				if (i == ranges.Count - 1 && 
					isDefaultCase == false && 
					statementBlock.HasBraces == false && 
					statementBlock.Statements.Count != 1)
				{
					indent++;
				}
				this.NewLine(sb);
			}
			if (IsDefaultCase)
			{
				sb.Append("default:");
				if (statementBlock.HasBraces == false && statementBlock.Statements.Count != 1)
				{
					indent++;
				}
				this.NewLine(sb);
			}
			statementBlock.ToSource(sb);

        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCaseLabel(this, data);
        }


	}
}
