using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class FixedBufferNode : MemberNode
	{
        public FixedBufferNode(Token relatedtoken)
            : base(relatedtoken)
		{
		}
        private NodeCollection<ConstantExpression> fixedBufferConstants = new NodeCollection<ConstantExpression>();
        public NodeCollection<ConstantExpression> FixedBufferConstants
        {
            get
            {
                return fixedBufferConstants;
            }
            set
            {
                fixedBufferConstants = value;
            }
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

			for (int i = 0; i < this.Names.Count; i++)
			{
				this.Names[i].ToSource(sb);
                sb.Append("[");
                FixedBufferConstants[i].ToSource(sb);
                sb.Append("]");
                sb.Append(", ");
			}
            sb.Remove(sb.Length - 2, 2);
			sb.Append(";");
			this.NewLine(sb);
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitFixedBufferNode(this, data);
        }

	}

}
