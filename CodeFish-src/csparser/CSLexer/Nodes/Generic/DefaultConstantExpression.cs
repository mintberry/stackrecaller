using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    /// <summary>
    /// To handle the default assignemet of a type paremeter in generic declaration
    /// i.e. : object x = default(T)
    /// </summary>
	public class DefaultConstantExpression : PrimaryExpression
	{
        TypeParameterNode typeParameter = null;

        public TypeParameterNode TypeParameter
        {
            get
            {
                return typeParameter;
            }
        }

        public DefaultConstantExpression(TypeParameterNode typeParameter)
            : base(typeParameter.RelatedToken)
		{
            this.typeParameter = typeParameter;
		}

		public override void ToSource(StringBuilder sb)
		{
            sb.Append("default(");
            typeParameter.ToSource(sb);
            sb.Append(")");
        }
        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDefaultValueExpression(this, data);
        }

	}
}
