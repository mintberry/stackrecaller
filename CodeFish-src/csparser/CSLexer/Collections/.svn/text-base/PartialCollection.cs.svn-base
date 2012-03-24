using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class PartialCollection : List<IPartial>, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
            foreach (ISourceCode p in this)
            {
                p.ToSource(sb);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (IPartial node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
