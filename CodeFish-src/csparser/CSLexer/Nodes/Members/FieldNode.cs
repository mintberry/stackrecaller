using System;
using System.Collections.Generic;
using System.Text;

namespace DDW
{
	public class FieldNode : MemberNode
	{
        public FieldNode(Token relatedtoken)
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

			this.type.ToSource(sb);
			sb.Append(" ");

			string comma = "";
			for (int i = 0; i < this.Names.Count; i++)
			{
				sb.Append(comma);
				comma = ", ";
				this.Names[i].ToSource(sb);
			}

			if (this.Value != null)
			{
				sb.Append(" = ");
				this.Value.ToSource(sb);
			}

			sb.Append(";");
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFieldDeclaration(this, data);
        }

	}

}
