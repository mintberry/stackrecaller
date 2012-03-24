using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class BooleanPrimitive : LiteralNode
	{
		public BooleanPrimitive(bool value, Token relatedToken) : base(relatedToken)
		{
			this.value = value;
		}

		private bool value;
		public bool Value
		{
			get { return this.value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(value.ToString().ToLower() + " ");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitBooleanPrimitive(this, data);
        }

	}
}
