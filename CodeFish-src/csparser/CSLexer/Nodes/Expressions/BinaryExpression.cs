using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using DotNetZen.CodeDom.Patterns;

namespace DDW
{
 
	public class BinaryExpression : ExpressionNode
	{
		private static SortedList<TokenID, string> stringMap;

        public BinaryExpression(Token relatedtoken)
            : base(relatedtoken)
		{
		}

        public BinaryExpression(TokenID op, Token relatedtoken)
            : base(relatedtoken)
		{
			this.op = op;
		}
        public BinaryExpression(TokenID op, ExpressionNode left)
            : base(left.RelatedToken)
		{
			this.op = op;
			this.left = left;
		}
        public BinaryExpression(TokenID op, ExpressionNode left, ExpressionNode right)
            : base(left.RelatedToken)
		{
			this.op = op;
			this.left = left;
			this.right = right; // right must be 'type'
		}
		private TokenID op;
		public TokenID Op
		{
			get { return op; }
			set 
			{
				if (!stringMap.ContainsKey(op))
				{
					throw new ArgumentException("The TokenID " + op + " does not represent a valid binary operator.");
				}
				op = value; 
			}
		}

		private ExpressionNode left;
		public ExpressionNode Left
		{
			get { return left; }
			set { left = value; }
		}

		private ExpressionNode right;
        public ExpressionNode Right
		{
			get { return right; }
			set { right = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			left.ToSource(sb);
			sb.Append(" " + stringMap[op] + " ");
			right.ToSource(sb);
		}

		static BinaryExpression()
		{
			stringMap = new  SortedList<TokenID, string>();
			stringMap.Add(TokenID.Not, @"!");
			stringMap.Add(TokenID.Percent, @"%");
			stringMap.Add(TokenID.BAnd, @"&");
			stringMap.Add(TokenID.BOr, @"|");
            stringMap.Add(TokenID.BXor, @"^");
			stringMap.Add(TokenID.Star, @"*");
			stringMap.Add(TokenID.Plus, @"+");
			stringMap.Add(TokenID.Minus, @"-");
			stringMap.Add(TokenID.Slash, @"/");
			stringMap.Add(TokenID.Less, @"<");
			stringMap.Add(TokenID.Equal, @"=");
			stringMap.Add(TokenID.Greater, @">");

			stringMap.Add(TokenID.PlusPlus, @"++");
			stringMap.Add(TokenID.MinusMinus, @"--");
			stringMap.Add(TokenID.And, @"&&");
			stringMap.Add(TokenID.Or, @"||");
			stringMap.Add(TokenID.EqualEqual, @"==");
			stringMap.Add(TokenID.NotEqual, @"!=");
			stringMap.Add(TokenID.LessEqual, @"<=");
			stringMap.Add(TokenID.GreaterEqual, @">=");
			stringMap.Add(TokenID.ShiftLeft, @"<<");
			stringMap.Add(TokenID.ShiftRight, @">>");

			stringMap.Add(TokenID.Is, @"is");
			stringMap.Add(TokenID.As, @"as");

			stringMap.Add(TokenID.MinusGreater, @"->");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBinaryOperatorExpression(this, data);
        }

	}
}
