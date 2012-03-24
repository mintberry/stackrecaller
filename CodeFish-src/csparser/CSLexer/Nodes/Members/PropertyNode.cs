using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class PropertyNode : MemberNode
	{
        public PropertyNode(Token relatedtoken)
            : base(relatedtoken)
		{
		}
		private AccessorNode getter;
		public AccessorNode Getter
		{
			get { return getter; }
			set { getter = value; }
		}

		private AccessorNode setter;
		public AccessorNode Setter
		{
            get { return setter; }
            set { setter = value; }
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
			indent++;
			this.NewLine(sb);

			if (getter != null)
			{
				getter.ToSource(sb);
			}
			if (setter != null)
			{
				setter.ToSource(sb);
			}

			indent--;
			this.NewLine(sb);
			sb.Append("}");
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitPropertyDeclaration(this, data);
        }
	}
}
