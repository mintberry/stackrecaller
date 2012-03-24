using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class InterfaceIndexerNode : MemberNode
	{
        public InterfaceIndexerNode(Token relatedToken)
            : base(relatedToken)
        {
        }
		private NodeCollection<ParamDeclNode> parameters;
		public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
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
			sb.Append("this [");
			if (parameters != null)
			{
				parameters.ToSource(sb);
			}
			sb.Append("]{");
			if (hasGetter)
			{
				sb.Append("get;");
			}
			if (hasSetter)
			{
				sb.Append("set;");
			}
			sb.Append("}");			
		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceIndexerNode(this, data);
        }

	}
}
