using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class WhileStatement : StatementNode
	{
        public WhileStatement(Token relatedToken)
            : base(relatedToken)
        {
            statements = new BlockStatement(RelatedToken);
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
            sb.Append("while (");
            test.ToSource( sb );
            sb.Append(") ");
            if ( statements.Statements.Count > 0 )
            {
                statements.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitWhileStatement(this, data);
        }


	}
}
