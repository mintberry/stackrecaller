using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class InterfaceEventNode : MemberNode
	{
        public InterfaceEventNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		public override void ToSource(StringBuilder sb)
		{
            if (attributes != null
                    && attributes.Count > 0)
			{
				attributes.ToSource(sb);
				this.NewLine(sb);
			}
			this.TraceModifiers(this.Modifiers, sb);

			sb.Append("event ");
			this.type.ToSource(sb);

			sb.Append(" ");
			this.names[0].ToSource(sb);

			sb.Append(";");
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceEventNode(this, data);
        }

	}
}
