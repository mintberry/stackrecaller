using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class LabeledStatement : StatementNode
	{
        public LabeledStatement(Token relatedtoken)
            : base(relatedtoken)
		{
		}
		private IdentifierExpression name;
		public IdentifierExpression Name
		{
			get { return name; }
			set { name = value; }
		}

		private NodeCollection<StatementNode> statements = new NodeCollection<StatementNode>();
		public NodeCollection<StatementNode> Statements
		{
			get { return statements; }
			set { statements = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			name.ToSource(sb);
			sb.Append(" : ");
			statements[0].ToSource(sb);
		}


        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLabelStatement(this, data);
        }

	}
}
