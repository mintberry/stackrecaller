using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class StringPrimitive : LiteralNode
	{
        public StringPrimitive(string value, Token relatedToken)
            : base(relatedToken)
		{
			this.value = value;
		}

		private string value;
		public string Value
		{
			get { return this.value; }
		}
		private bool isVerbatim = true; // strings are always lexed as verbatim for now
		public bool IsVerbatim
		{
			get { return isVerbatim; }
			set { isVerbatim = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if (value != null)
			{
				if (isVerbatim)
				{
					sb.Append("@");
				}
				sb.Append("\"" + value + "\"");
			}
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitStringPrimitive(this, data);
        }

	}    
}
