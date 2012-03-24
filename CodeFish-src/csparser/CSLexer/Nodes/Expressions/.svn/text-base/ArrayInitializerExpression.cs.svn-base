using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class ArrayInitializerExpression : ExpressionNode
	{
        public ArrayInitializerExpression(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private ExpressionList expressions;
		public ExpressionList Expressions
		{
			get { return expressions; }
			set { expressions = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("{");
			expressions.ToSource(sb);
			sb.Append("}");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArrayInitializerExpression(this, data);
        }

	}
}
