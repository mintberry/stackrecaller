using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class DoStatement : StatementNode
	{
        public DoStatement(Token relatedToken)
            : base(relatedToken)
        {
            statements = new BlockStatement(relatedToken);
        }
		private ExpressionNode test;
		public ExpressionNode Test
		{
			get { return test; }
			set { test = value; }
		}

        private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("do ");
            statements.ToSource(sb);
            sb.Append("while (");
            test.ToSource(sb);
            sb.Append(");");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDoLoopStatement(this, data);
        }

	}
}
