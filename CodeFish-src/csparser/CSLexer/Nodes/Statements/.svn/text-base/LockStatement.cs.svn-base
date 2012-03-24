using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class LockStatement : StatementNode
	{
        public LockStatement(Token relatedtoken)
            : base(relatedtoken)
        {
            statements = new BlockStatement(relatedtoken);
        }
		private ExpressionNode target;
		public ExpressionNode Target
		{
			get { return target; }
			set { target = value; }
		}
		private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("lock(");
			target.ToSource(sb);
			sb.Append(")");
			statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLockStatement(this, data);
        }
	}
}
