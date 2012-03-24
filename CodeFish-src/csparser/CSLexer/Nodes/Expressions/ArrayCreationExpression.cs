using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;
using System.CodeDom;

namespace DDW
{
	public class ArrayCreationExpression : PrimaryExpression
	{
        public ArrayCreationExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
		private IType type;
        public IType Type
		{
			get { return type; }
			set { type = value; }
		}
		private ExpressionList rankSpecifier;
		public ExpressionList RankSpecifier
		{
			get { return rankSpecifier; }
			set { rankSpecifier = value; }
		}

		private List<int> additionalRankSpecifiers = new List<int>();
		public List<int> AdditionalRankSpecifiers
		{
			get { return additionalRankSpecifiers; }
			set { additionalRankSpecifiers = value; }
		}

		private ArrayInitializerExpression initializer;
		public ArrayInitializerExpression Initializer
		{
			get { return initializer; }
			set { initializer = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
            // Omer - can this class support parsing of int[, ,][,][]?

			sb.Append("new ");
			type.ToSource(sb);

			if (rankSpecifier != null && rankSpecifier.Expressions.Count > 0)
			{
                sb.Append("[");
				rankSpecifier.ToSource(sb);
                sb.Append("]");
			}			

			if (additionalRankSpecifiers != null)
			{
				for (int i = 0; i < additionalRankSpecifiers.Count; i++)
				{					
					sb.Append("[");
					for (int j = 0; j < additionalRankSpecifiers[i]; j++)
					{						
						sb.Append(",");
					}
					sb.Append("]");
				}
			}
			if (initializer != null)
			{
				initializer.ToSource(sb);
			}
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitArrayCreateExpression(this, data);
        }

	}
}
