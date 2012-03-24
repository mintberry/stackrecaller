using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class CheckedStatement : StatementNode
	{
        public CheckedStatement(Token relatedToken)
            : base(relatedToken)
        {
        }
        private ExpressionNode checkedExpression = null;
        public ExpressionNode CheckedExpression
        {
            get
            {
                return checkedExpression;
            }
            set
            {
                checkedExpression = value;
            }
        }

		private BlockStatement checkedBlock = null;
		public BlockStatement CheckedBlock
		{
			get { return checkedBlock; }
			set { checkedBlock = value; }
        }

        public override void ToSource(StringBuilder sb)
        {

            sb.Append("checked ");

            if (checkedExpression != null)
            {
                sb.Append("(");
                checkedExpression.ToSource(sb);
                sb.Append(")");
                sb.Append(";");
            }
            else
            {
                if (checkedExpression != null)
                {
                    checkedBlock.ToSource(sb);
                }
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitCheckedStatement(this, data);
        }

	}
}
