using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class ForEachStatement : StatementNode
	{
        public ForEachStatement(Token relatedtoken)
            : base(relatedtoken)
        {
            statements = new BlockStatement(relatedtoken);
        }
		private ParamDeclNode iterator;
		public ParamDeclNode Iterator
		{
			get { return iterator; }
			set { iterator = value; }
		}
		private ExpressionNode collection;
		public ExpressionNode Collection
		{
			get { return collection; }
			set { collection = value; }
		}

        private BlockStatement statements;
		public BlockStatement Statements
		{
			get { return statements; }
			set { statements = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("foreach (");

            iterator.ToSource(sb);

            sb.Append(" in ");

            collection.ToSource(sb);

            sb.Append( ")" );

            sb.Append(Environment.NewLine);

            if (statements.Statements.Count > 0)
            {
                statements.ToSource(sb);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitForeachStatement(this, data);
        }

	}
}
