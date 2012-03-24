using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class UsingStatement : StatementNode
	{
        public UsingStatement(Token relatedToken)
            : base(relatedToken)
        {
            statements = new BlockStatement(RelatedToken);
        }
		private ExpressionNode resource;
		public ExpressionNode Resource
		{
			get { return resource; }
			set { resource = value; }
		}
        private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("using(");
			resource.ToSource(sb);
			sb.Append(")");
			statements.ToSource(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUsingStatement(this, data);
        }

	}
}
