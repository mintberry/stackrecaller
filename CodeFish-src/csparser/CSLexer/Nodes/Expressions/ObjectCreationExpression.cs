using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class ObjectCreationExpression : PrimaryExpression
	{
        public ObjectCreationExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public ObjectCreationExpression(IType type, Token relatedToken)
            : base(relatedToken)
		{
			this.type = type;
		}
        public ObjectCreationExpression(IType type, ExpressionList argumentList, Token relatedToken)
            : base(relatedToken)
		{
			this.type = type;
			this.argumentList = argumentList;
		}

		private IType type;
        public IType Type
		{
			get { return type; }
			set { type = value; }
		}

		private ExpressionList argumentList;
		public ExpressionList ArgumentList
		{
			get { return argumentList; }
			set { argumentList = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append("new ");
			type.ToSource(sb);
			sb.Append("(");
			argumentList.ToSource(sb);
			sb.Append(")");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitObjectCreateExpression(this, data);
        }
	}
}
