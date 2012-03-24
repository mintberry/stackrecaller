using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class InterfacePropertyNode : MemberNode
	{
        public InterfacePropertyNode(Token relatedtoken)
            : base(relatedtoken)
        {
        }
		private bool hasGetter;
		public bool HasGetter
		{
			get { return hasGetter; }
			set { hasGetter = value; }
		}

		private bool hasSetter;
		public bool HasSetter
		{
			get { return hasSetter; }
			set { hasSetter = value; }
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

			this.type.ToSource(sb);
			sb.Append(" ");

			this.names[0].ToSource(sb);

			// start block
			this.NewLine(sb);
			sb.Append("{");

			if (hasGetter)
			{
				sb.Append("get; ");
			}
			if (hasSetter)
			{
				sb.Append("set; ");
			}

			sb.Append("}");
			this.NewLine(sb);
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfacePropertyNode(this, data);
        }

	}
}
