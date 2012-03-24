using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class MemberAccessExpression : PrimaryExpression, IType
	{
        public MemberAccessExpression(Token relatedToken)
            : base(relatedToken)
        {
        }
        public MemberAccessExpression(IMemberAccessible left, ExpressionNode right)
            : base(((BaseNode)left).RelatedToken)
		{
			this.left = left;
			this.right = right;
		}

        private bool isNamespaceAliasQualifier = false;
        public bool IsNamespaceAliasQualifier
        {
            get
            {
                return isNamespaceAliasQualifier;
            }
            set
            {
                isNamespaceAliasQualifier = value;
            }
        }

		private ExpressionNode right;
        public ExpressionNode Right
		{
            get { return right; }
            set { right = value; }
		}

		private IMemberAccessible left;
		public IMemberAccessible Left
		{
			get { return left; }
			set { left = value; }
		}

		private List<int> rankSpecifiers = new List<int>();
		public List<int> RankSpecifiers
		{
			get { return rankSpecifiers; }
			set { rankSpecifiers = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			left.ToSource(sb);

            if (!(left is DereferenceExpression)
                || (left as DereferenceExpression).Star)
            {
                if (isNamespaceAliasQualifier)
                {
                    sb.Append("::");
                }
                else
                {
                    sb.Append(".");
                }
            }

			right.ToSource(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitMemberAccessExpression(this, data);
        }

	}
}
