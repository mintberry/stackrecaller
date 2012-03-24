using System;
using System.Collections.Generic;
using System.Text;

namespace DDW.Collections
{
	public class ExpressionList : ExpressionNode, IEnumerable<ExpressionNode>
	{
        public ExpressionList()
            : base(new Token( TokenID.Invalid) )
        {
        }

		private NodeCollection<ExpressionNode> expressions = new NodeCollection<ExpressionNode>();
		public NodeCollection<ExpressionNode> Expressions
		{
			get { return expressions; }
		}

		public override void ToSource(StringBuilder sb)
		{
			string comma = "";
			foreach(ExpressionNode node in expressions)
			{
				sb.Append(comma);
				comma = ", ";
				node.ToSource(sb);
			}
		}

        #region IEnumerable<ExpressionNode> Members

        IEnumerator<ExpressionNode> IEnumerable<ExpressionNode>.GetEnumerator()
        {
            return this.expressions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.expressions.GetEnumerator();
        }

        #endregion

        public int Count
        {
            get
            {
                return Expressions.Count;
            }
        }

        public ExpressionNode this[int idx]
        {
            get
            {
                return Expressions[idx];
            }
        }

        public ExpressionNode Last
        {
            get
            {
                if (Count > 0)
                {
                    return this[this.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }

        public void Add(ExpressionNode expr)
        {
            this.Expressions.Add(expr);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return expressions.AcceptVisitor(visitor, data);
        } 
    }
}
