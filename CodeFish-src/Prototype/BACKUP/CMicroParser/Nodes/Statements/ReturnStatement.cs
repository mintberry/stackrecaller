using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ReturnStatement : StatementNode
	{
        public ReturnStatement(Token relatedtoken)
            : base(relatedtoken)
		{
		}
		private ExpressionNode returnValue;
		public ExpressionNode ReturnValue
		{
			get { return returnValue; }
			set { returnValue = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			sb.Append("return");
			if (returnValue != null)
			{
				sb.Append(" ");
				returnValue.ToSource(sb);
			}
			sb.Append(";");
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitReturnStatement(this, data);
        }
	}
}
