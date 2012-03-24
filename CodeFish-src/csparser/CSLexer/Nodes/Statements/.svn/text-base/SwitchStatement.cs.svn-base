using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class SwitchStatement : StatementNode
	{
        public SwitchStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private ExpressionNode test;
		public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

		private NodeCollection<CaseNode> cases = new NodeCollection<CaseNode>();
		public NodeCollection<CaseNode> Cases
		{
			get { return cases; }
			set { cases = value; }
		}

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("switch(");
			test.ToSource(sb);
			sb.Append(")");

			this.NewLine(sb);
			sb.Append("{");
			indent++;

			for (int i = 0; i < cases.Count; i++)
			{
				cases[i].ToSource(sb);
			}
			indent--;
			this.NewLine(sb);
			sb.Append("}");
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitSwitchStatement(this, data);
        }
        
	}
}
