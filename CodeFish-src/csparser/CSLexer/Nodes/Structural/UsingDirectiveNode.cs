using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class UsingDirectiveNode : BaseNode
	{
        public UsingDirectiveNode(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        private PrimaryExpression target;
		public PrimaryExpression Target
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return target; }
            [System.Diagnostics.DebuggerStepThrough]
            set { target = value; }
		}

		private QualifiedIdentifierExpression aliasName = null;
        public QualifiedIdentifierExpression AliasName
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return aliasName; }
            [System.Diagnostics.DebuggerStepThrough]
            set { aliasName = value; }
		}

		public bool IsAlias
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return (aliasName != null); }
        }

        public override void ToSource(StringBuilder sb)
        {
			sb.Append("using ");

            if (IsAlias)
            {
                aliasName.ToSource(sb);
                sb.Append(" = ");
            }

			// target
			target.ToSource(sb);

			sb.Append(";");
			this.NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (this.IsAlias)
            {
                throw new NotSupportedException();
            }
            else
            {
                resolver.Context.AddUsingDirective(this);
            }
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitUsingDirective(this, data);
        }

	}
}
