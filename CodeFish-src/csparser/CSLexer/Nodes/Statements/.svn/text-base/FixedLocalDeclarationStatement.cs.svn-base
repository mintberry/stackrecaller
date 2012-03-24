using System;
using System.Collections.Generic;
using System.Text;

using DDW.Collections;

namespace DDW
{
	public class FixedDeclarationsStatement : ExpressionNode
	{

        public FixedDeclarationsStatement(Token relatedtoken)
            : base(relatedtoken)
        {
        }
 
		IType type;
		public IType Type
		{
			get { return type; }
			set { type = value; }
		}

        ExpressionList  identifiers = new ExpressionList();
        public ExpressionList  Identifiers
		{
			get { return identifiers; }
		}

        ExpressionList  rightSide = new ExpressionList();
        public ExpressionList  RightSide
		{
			get { return rightSide; }
		}


		public override void ToSource(StringBuilder sb)
		{
			type.ToSource(sb);
			sb.Append(" ");

            for (int i = 0; i < identifiers.Expressions.Count; i++)
			{
                identifiers.Expressions[i].ToSource(sb);

                // i do not control if 'i' is inferior 
                // to rightSide.Count, because the initializer can not be empty
                // so if there is no initializer for a pointer declarator
                // it means that there is something wrong and the parser will throw an exception
                //
                // -> this is a good thing to know that something is wrong, without re read all 
                // output
                sb.Append("=");
                rightSide.Expressions[i].ToSource(sb);

                sb.Append(", ");
			}

            sb.Remove(sb.Length - 2, 2);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedDeclarationStatement(this, data);
        }

	}
}
