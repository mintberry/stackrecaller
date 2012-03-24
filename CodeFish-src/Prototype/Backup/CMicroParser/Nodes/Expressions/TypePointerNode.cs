using System;
using System.Collections.Generic;
using System.Text;

using System.CodeDom;

namespace DDW
{
    public class TypePointerNode : PrimaryExpression, IEquatable<TypePointerNode>, IPointer, IType, INullableType
	{
        private ExpressionNode expression = null;

        public ExpressionNode Expression
        {
            get
            {
                return expression;
            }
        }

        public TypePointerNode(Token relatedToken)
            : base(relatedToken)
        {
        }
        public TypePointerNode(ExpressionNode expression): base(expression.RelatedToken)
		{
            this.expression = expression;
		}

        bool isNullableType = false;
        public bool IsNullableType
        {
            get
            {
                return isNullableType;
            }
            set
            {
                this.isNullableType = true;
            }
        }

        private List<int> rankSpecifiers = new List<int>();
        public List<int> RankSpecifiers
        {
            get { return rankSpecifiers; }
            set { rankSpecifiers = value; }
        }

        public bool Equals(TypePointerNode other)
        {
            bool ret = false;

            if (this == other)
            {
                ret = true;
            }
            else
            {
                if (other != null)
                {
                    if (this.expression == null && other.expression == null
                        || expression != null && expression.Equals(other.expression))
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public override bool Equals(object obj)
        {
            bool ret = false;

            if (obj is TypePointerNode)
            {
                ret = Equals(obj as TypePointerNode);
            }
            else
            {
                ret = base.Equals(obj);
            }

            return ret;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

		public override void ToSource(StringBuilder sb)
		{
            expression.ToSource(sb);

             sb.Append("*");

             if (isNullableType)
             {
                 sb.Append("?");
             }

             if (rankSpecifiers.Count > 0)
             {
                 foreach (int val in rankSpecifiers)
                 {
                     sb.Append("[");
                     for (int i = 0; i < val; i++)
                     {
                         sb.Append(",");
                     }
                     sb.Append("]");
                 }
             }
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitTypePointerReference(this, data);
        }
	}
}
