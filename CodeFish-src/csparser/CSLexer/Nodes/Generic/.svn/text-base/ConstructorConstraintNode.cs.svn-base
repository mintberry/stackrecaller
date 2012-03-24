using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
    /// <summary>
    /// constructor-constraint:
    ///     new   (   )
    /// </summary>
	public sealed class ConstructorConstraint : ISourceCode
	{

        public ConstructorConstraint()
        {
        }

		public void ToSource(StringBuilder sb)
		{
            sb.Append("new() ");
		}

        public object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitConstructorConstraint(this, data);
        }
	}
}
