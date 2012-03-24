using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class DecimalPrimitive : LiteralNode
	{
        public DecimalPrimitive(decimal value, Token relatedToken)
            : base(relatedToken)
		{
			this.value = value;
		}

		private decimal value;
		public decimal Value
		{
			get { return this.value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(value + " ");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDecimalPrimitive(this, data);
        }

	}
}
