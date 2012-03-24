using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class IfStatement : StatementNode
	{
        public IfStatement(Token relatedToken)
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
		}

		private BlockStatement elseStatements;
		public BlockStatement ElseStatements
		{
            get { if (elseStatements == null) elseStatements = new BlockStatement(RelatedToken); return elseStatements; }
       }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("if(");
			if (test != null)
			{
				test.ToSource(sb);
			}
			sb.Append(")");
			this.NewLine(sb);
			this.statements.ToSource(sb);

			if (this.elseStatements != null)
			{
				sb.Append("else ");
				elseStatements.ToSource(sb);
			}
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitIfStatement(this, data);
        }

	}
}
