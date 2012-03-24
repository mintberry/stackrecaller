using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class ExternAliasDirectiveNode : BaseNode
	{
        public ExternAliasDirectiveNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
        private ExpressionNode externaAliasName;
        public ExpressionNode ExternAliasName
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { return externaAliasName; }
            [System.Diagnostics.DebuggerStepThrough]
            set { externaAliasName = value; }
		}
        public override void ToSource(StringBuilder sb)
        {
			sb.Append("extern alias ");

            if (externaAliasName != null)
            {
                externaAliasName.ToSource(sb);
            }

			sb.Append(";");
			this.NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, canEnterContext);

            throw new NotSupportedException();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitExternAliasDirectiveNode(this, data);
        }

	}
}
