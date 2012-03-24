using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class UncheckedStatement : StatementNode
    {
        public UncheckedStatement(Token relatedToken)
            : base(relatedToken)
        {
        }
        private ExpressionNode uncheckedExpression = null;
        public ExpressionNode UncheckedExpression
        {
            get
            {
                return uncheckedExpression;
            }
            set
            {
                uncheckedExpression = value;
            }
        }

		private BlockStatement uncheckedBlock = null;
		public BlockStatement UncheckedBlock
		{
			get { return uncheckedBlock; }
			set { uncheckedBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("unchecked ");

            if (uncheckedExpression != null)
            {
                sb.Append("(");
                uncheckedExpression.ToSource(sb);
                sb.Append(")");
                sb.Append(";");
            }
            else
            {
                if (uncheckedBlock != null)
                {
                    uncheckedBlock.ToSource(sb);
                }
            }            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUncheckedStatement(this, data);
        }

	}
}
