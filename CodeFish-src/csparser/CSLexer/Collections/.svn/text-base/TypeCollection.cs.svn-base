using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class TypeCollection : List<IType>, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
            foreach (ISourceCode t in this)
            {
                t.ToSource(sb);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (IType node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
