using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.CodeDom;

namespace DDW
{
	// todo: need to add ValueAsDecimal, ValueAsDouble, and ValueAsFloat, then store a string and 'kind'.
	public class RealPrimitive : LiteralNode
	{
		private string originalString;
        public RealPrimitive(string value, Token relatedToken)
            : base(relatedToken)
		{
			this.originalString = value;
			char c = value[value.Length - 1];
			switch(c)
			{
				case 'f':
				case 'F':
					isFloat = true;
					value = value.Substring(0, value.Length - 1);
					val = (double)float.Parse(value, CultureInfo.InvariantCulture);
					break;
				case 'd':
				case 'D':
					isDouble = true;
					value = value.Substring(0, value.Length - 1);
					val = double.Parse(value, CultureInfo.InvariantCulture);
					break;
				case 'm':
				case 'M':
					isDecimal = true;
					value = value.Substring(0, value.Length - 1);
					val = (double)decimal.Parse(value, CultureInfo.InvariantCulture);
					break;
				default:
					val = double.Parse(value, CultureInfo.InvariantCulture);
					break;
			}
		}
        public RealPrimitive(double value, Token relatedToken)
            : base(relatedToken)
		{
			isDouble = true;
			this.val = value;
		}

		private double val;
		public double Value
		{
			get { return this.val; }
		}
		private bool isFloat = false;
		public bool IsFloat
		{
			get { return isFloat; }
			set { isFloat = value; }
		}
		private bool isDouble;
		public bool IsDouble
		{
			get { return isDouble; }
			set { isDouble = value; }
		}

		private bool isDecimal;
		public bool IsDecimal
		{
			get { return isDecimal; }
			set { isDecimal = value; }
		}
	

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(val.ToString( CultureInfo.InvariantCulture) );
			if (isFloat)
			{
				sb.Append("f");
			}
			else if (isDouble)
			{
				sb.Append("d");
			}
			else if (isDecimal)
			{
				sb.Append("m");
			}
		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitRealPrimitive(this, data);
        }

	}
}
