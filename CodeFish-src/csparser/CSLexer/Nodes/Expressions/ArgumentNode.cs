using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ArgumentNode : BaseNode
	{
        public ArgumentNode(Token relatedToken)
            : base(relatedToken)
        {
        }

		private bool isRef;
		public bool IsRef
		{
			get { return isRef; }
			set { isRef = value; }
		}

		private bool isOut;
		public bool IsOut
		{
			get { return isOut; }
			set { isOut = value; }
		}

		private ExpressionNode expression;
		public ExpressionNode Expression
		{
			get { return expression; }
			set { expression = value; }
        }

        public override void ToSource(StringBuilder sb)
		{
			if (isRef)
			{
				sb.Append("ref ");
			}
			else if (IsOut)
			{
				sb.Append("out ");
			}

			expression.ToSource(sb);
            
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArgumentNode(this, data);
        }

	}
}
