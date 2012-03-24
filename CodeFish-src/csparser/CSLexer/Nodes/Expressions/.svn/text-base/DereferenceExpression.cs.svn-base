using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;

namespace DDW
{
	public class DereferenceExpression : PrimaryExpression
	{
        /// <summary>
        /// used to knwo if the kind of dereference : 
        /// *a
        /// or 
        /// a->b
        /// 
        /// It changes only the ToSource method
        /// </summary>
        private bool star = true;

        public bool Star
        {
            get
            {
                return star;
            }
        }

        public DereferenceExpression(Token relatedToken)
            : base(relatedToken)
        {
        }


        public DereferenceExpression(ExpressionNode expression, bool star)
            : base(expression.RelatedToken)
		{
            this.expression = expression;
            this.star = star;
		}

        private ExpressionNode expression;
        public ExpressionNode Expression
		{
            get { return expression; }
            set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if ( star ) sb.Append("*");
            expression.ToSource(sb);
            if (!star) sb.Append("->");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitDereferenceExpression(this, data);
        }

	}
}
