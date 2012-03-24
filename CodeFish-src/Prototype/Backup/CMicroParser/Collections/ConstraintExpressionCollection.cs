using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class ConstraintExpressionCollection : List<ConstraintExpressionNode>, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
            foreach (ConstraintExpressionNode ce in this)
            {
                ce.ToSource(sb);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (ConstraintExpressionNode node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
