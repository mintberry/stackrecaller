using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class AttributeArgumentNode : BaseNode
	{
        public AttributeArgumentNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private IdentifierExpression argumentName;
		public IdentifierExpression ArgumentName
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return argumentName; }
            [System.Diagnostics.DebuggerStepThrough]
            set { argumentName = value; }
		}

		private ExpressionNode expression;
		public ExpressionNode Expression
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return expression; }
            [System.Diagnostics.DebuggerStepThrough]
            set { expression = value; }
		}

		public override void ToSource(StringBuilder sb)
		{
			if (argumentName != null)
			{
				argumentName.ToSource(sb);
				sb.Append("= ");
			}
			expression.ToSource(sb);
		}

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            if (this.expression != null)
            {
                this.expression.Parent = this;
                this.expression.Resolve(resolver);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitAttributeArgumentNode(this, data);
        }

	}
}
