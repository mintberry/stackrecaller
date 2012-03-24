using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
	public class IndexerNode : MemberNode
	{
        public IndexerNode(Token relatedToken)
            : base(relatedToken)
        {
        }
		private TypeNode interfaceType;
		public TypeNode InterfaceType
		{
			get { return interfaceType; }
			set { interfaceType = value; }
		}

		private NodeCollection<ParamDeclNode> parameters;
		public NodeCollection<ParamDeclNode> Params
		{
			get { if (parameters == null) parameters = new NodeCollection<ParamDeclNode>(); return parameters; }
			set { parameters = value; }
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
			TraceModifiers(modifiers, sb);

			type.ToSource(sb);
			sb.Append(" ");
			if (interfaceType != null)
			{
				interfaceType.ToSource(sb);
				sb.Append(".");
			}
			sb.Append("this[");
			if (parameters != null)
			{
				string comma = "";
				foreach (ParamDeclNode pdn in parameters)
				{
					sb.Append(comma);
					comma = ", ";
					pdn.ToSource(sb);
				}
			}
			sb.Append("]");

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
            return visitor.VisitIndexerDeclaration(this, data);
        }

	}

}
