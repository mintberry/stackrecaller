using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class ConstraintCollection : List<Constraint>, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
            foreach (Constraint constraint in this)
            {
                constraint.ToSource(sb);
                sb.Append(Environment.NewLine);
            }
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (Constraint node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
