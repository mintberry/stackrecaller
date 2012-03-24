using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class GotoStatement : StatementNode
	{
        public GotoStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private bool isCase = false;
		public bool IsCase
		{
			get { return isCase; }
			set { isCase = value; }
		}
		private bool isDefaultCase = false;
		public bool IsDefaultCase
		{
			get { return isDefaultCase; }
			set { isDefaultCase = value; }
		}

		private ExpressionNode target;
		public ExpressionNode Target
		{
			get { return target; }
			set { target = value; }
        }

        public override void ToSource(StringBuilder sb)
        {
            sb.Append("goto ");

            if (IsDefaultCase)
            {
                sb.Append("default");
            }
            else
            {
                if (isCase)
                {
                    sb.Append("case ");
                }

                target.ToSource(sb);
            }

            sb.Append(";");
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitGotoStatement(this, data);
        }

	}
}
