using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
    public class TypeParameterCollection : List<TypeParameterNode>, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
            foreach (TypeParameterNode tp in this)
            {
                tp.ToSource(sb);
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1);
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (TypeParameterNode tp in this)
            {
                tp.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
