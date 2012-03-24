using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
	public class NodeCollection<T> : List<T> where T : BaseNode, ISourceCode
	{
		public void ToSource(StringBuilder sb)
		{
			foreach (BaseNode node in this)
			{
				node.ToSource(sb);
			}
		}

        public virtual object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            foreach (BaseNode node in this)
            {
                node.AcceptVisitor(visitor, data);
            }

            return null;
        } 
	}
}
