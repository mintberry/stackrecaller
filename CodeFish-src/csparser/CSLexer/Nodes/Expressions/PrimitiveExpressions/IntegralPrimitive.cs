using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.CodeDom;

namespace DDW
{
	// todo: need to add ValueAsUInt, ValueAsULong, and ValueAsByte etc, then store a string and 'kind'.
	public class IntegralPrimitive : LiteralNode
	{
		private string originalString;
		public IntegralPrimitive(string value, IntegralType integralType, Token relatedToken) : base(relatedToken)
		{
			this.originalString = value;
			this.integralType = integralType;

			switch (integralType)
			{
				case IntegralType.SByte:
				case IntegralType.Byte:
				case IntegralType.Short:
				case IntegralType.Int:
					break;

				case IntegralType.UShort:
					value = value.TrimEnd('U', 'u');
					break;

				case IntegralType.UInt:
					value = value.TrimEnd('U', 'u');
					break;

				case IntegralType.Long:
					value = value.TrimEnd('L', 'l');
					break;

				case IntegralType.ULong:
					value = value.TrimEnd('L', 'l', 'U', 'u');
					value = value.TrimEnd('L', 'l', 'U', 'u');
					break;

				default:
					throw new FormatException("Illegal Integral type");
			}

			int radix = 10;
			NumberStyles style = NumberStyles.Integer;
			if (value.StartsWith("0x", true, CultureInfo.InvariantCulture))
			{
				radix = 16;
				style = NumberStyles.HexNumber;
				value = value.Substring(2, value.Length - 2);
			}
			// negation is wrapped in a unaryNegationNode so no need to account for negative values
			//try
			//{
			this.value = UInt64.Parse(value, style);
			//this.value = Convert.ToUInt64(value, radix);

			//}
			//catch (OverflowException)
			//{
			//    ConsoleWr
			//}
			//catch (FormatException)
			//{
			//}

		}

		private ulong value;
		public ulong Value
		{
			get { return this.value; }
		}

		private IntegralType integralType;
		public IntegralType IntegralType
		{
			get { return integralType; }
		}

		public override void ToSource(StringBuilder sb)
		{
			sb.Append(value);

            switch (integralType)
            {
                case IntegralType.SByte:
                case IntegralType.Byte:
                case IntegralType.Short:
                case IntegralType.Int:
                    break;

                case IntegralType.UShort:
                case IntegralType.UInt:
                    sb.Append('U');
                    break;

                case IntegralType.Long:
                    sb.Append('L');
                    break;

                case IntegralType.ULong:
                    sb.Append('U');
                    break;

                default:
                    throw new FormatException("Illegal Integral type");
            }

		}
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitIntegralPrimitive(this, data);
        }

	}
}
