using System;
using System.Collections.Generic;
using System.Text;

using DDW.Collections;

namespace DDW
{
    // Omer - why does this inherit from ExpressionNode? This is not even a statement - it's a member...
	public class LocalDeclarationStatement : ExpressionNode
	{
        public LocalDeclarationStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        public LocalDeclarationStatement(IType type, IdentifierExpression identifier, ExpressionNode rightSide)
            : base(identifier.RelatedToken)
		{
			this.type = type;
            this.identifiers.Expressions.Add(identifier);
			this.rightSide = rightSide;
		}

		IType type;
		public IType Type
		{
			get { return type; }
			set { type = value; }
		}

        ExpressionList identifiers = new ExpressionList();
        public ExpressionList Identifiers
		{
			get { return identifiers; }
			set { identifiers = value; }
		}

		ExpressionNode rightSide;
		public ExpressionNode RightSide
		{
			get { return rightSide; }
			set { rightSide = value; }
		}

		private bool isConstant = false;
		public bool IsConstant
		{
			get { return isConstant; }
			set { isConstant = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if (isConstant)
			{
				sb.Append("const ");
			}
			type.ToSource(sb);
			sb.Append(" ");
			string comma = "";
			for (int i = 0; i < identifiers.Expressions.Count; i++)
			{
				sb.Append(comma);
				identifiers.Expressions[i].ToSource(sb);
				comma = ", ";
			}

			if (rightSide != null)
			{
				sb.Append(" = ");
				rightSide.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitLocalDeclarationStatement(this, data);
        }

	}
}
