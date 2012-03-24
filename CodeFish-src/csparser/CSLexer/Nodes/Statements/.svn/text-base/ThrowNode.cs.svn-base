using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ThrowNode : StatementNode
	{
        public ThrowNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private ExpressionNode throwExpression;
		public ExpressionNode ThrowExpression
		{
			get { return throwExpression; }
			set { throwExpression = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("throw ");
			if (throwExpression != null)
			{
				throwExpression.ToSource(sb);
			}
			sb.Append(";");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitThrowStatement(this, data);
        }
	}
}
