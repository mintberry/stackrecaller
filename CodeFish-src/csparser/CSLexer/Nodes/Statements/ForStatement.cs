using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ForStatement : StatementNode
	{
        public ForStatement(Token relatedtoken)
            : base(relatedtoken)
        {
            statements = new BlockStatement(relatedtoken);
        }
		private NodeCollection<ExpressionNode> init = new NodeCollection<ExpressionNode>();
		public NodeCollection<ExpressionNode> Init
		{
			get { return init; }
			set { init = value; }
		}
		private NodeCollection<ExpressionNode> test = new NodeCollection<ExpressionNode>();
		public NodeCollection<ExpressionNode> Test
		{
			get { return test; }
			set { test = value; }
		}
		private NodeCollection<ExpressionNode> inc = new NodeCollection<ExpressionNode>();
		public NodeCollection<ExpressionNode> Inc
		{
			get { return inc; }
			set { inc = value; }
		}

		private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("for(");
			init.ToSource(sb);
			sb.Append("; ");
			test.ToSource(sb);
			sb.Append("; ");
			inc.ToSource(sb);
			sb.Append(")");
			this.NewLine(sb);
			statements.ToSource(sb);
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitForStatement(this, data);
        }

	}
}
