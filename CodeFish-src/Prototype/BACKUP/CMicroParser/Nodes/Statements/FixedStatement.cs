using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class FixedStatementStatement : StatementNode
	{
        public FixedStatementStatement(Token relatedtoken)
            : base(relatedtoken)
        {
            localPointers = new FixedDeclarationsStatement(relatedtoken);
            statements = new BlockStatement(relatedtoken);
        }
        FixedDeclarationsStatement localPointers;
        public FixedDeclarationsStatement LocalPointers
        {
            get
            {
                return localPointers;
            }
        }

        private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
		}

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("fixed(");

            localPointers.ToSource(sb);

			sb.Append(")");
			this.NewLine(sb);
			this.statements.ToSource(sb);            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedStatement(this, data);
        }

	}
}
