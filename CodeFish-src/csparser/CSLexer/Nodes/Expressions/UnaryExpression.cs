using System;
using System.Collections.Generic;
using System.Text;
using DotNetZen.CodeDom.Patterns;
using DotNetZen.CodeDom;

namespace DDW
{
	public class UnaryExpression : ExpressionNode
	{
		private static SortedList<TokenID, string> stringMap;

        public UnaryExpression(Token relatedtoken)
            : base(relatedtoken)
		{
		}

        public UnaryExpression(TokenID op, Token relatedtoken) : base(relatedtoken)
		{
			this.op = op;
		}
        public UnaryExpression(TokenID op, ExpressionNode child, Token relatedtoken)
            : base(relatedtoken)
		{
			this.op = op;
			this.child = child;
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

		protected ExpressionNode child;
		public ExpressionNode Child
		{
			get { return child; }
			set { child = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(stringMap[op]);
			child.ToSource(sb);
		}

		static UnaryExpression()
		{
			stringMap = new  SortedList<TokenID, string>();
			stringMap.Add(TokenID.Tilde, "~");
			stringMap.Add(TokenID.Minus, @"-");
			stringMap.Add(TokenID.Not, @"!");
			stringMap.Add(TokenID.Plus, @"+");
			stringMap.Add(TokenID.PlusPlus, @"++");
			stringMap.Add(TokenID.MinusMinus, @"--");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUnaryExpression(this, data);
        }
	}
}
