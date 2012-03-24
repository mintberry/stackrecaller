using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class InterfaceNode : ConstructedTypeNode
	{
		private TypeCollection baseClasses;
        public TypeCollection BaseClasses
		{
            get { if (baseClasses == null) baseClasses = new TypeCollection(); return baseClasses; }
		}

		private NodeCollection<InterfaceMethodNode> methods;
		public NodeCollection<InterfaceMethodNode> Methods
		{
			get { if (methods == null) methods = new NodeCollection<InterfaceMethodNode>(); return methods; }
			set { methods = value; }
		}

		private NodeCollection<InterfacePropertyNode> properties;
		public NodeCollection<InterfacePropertyNode> Properties
		{
			get { if (properties == null) properties = new NodeCollection<InterfacePropertyNode>(); return properties; }
			set { properties = value; }
		}

		private NodeCollection<InterfaceIndexerNode> indexers;
		public NodeCollection<InterfaceIndexerNode> Indexers
		{
			get { if (indexers == null) indexers = new NodeCollection<InterfaceIndexerNode>(); return indexers; }
			set { indexers = value; }
		}

		private NodeCollection<InterfaceEventNode> events;
		public NodeCollection<InterfaceEventNode> Events
		{
			get { if (events == null) events = new NodeCollection<InterfaceEventNode>(); return events; }
			set { events = value; }
		}

        public InterfaceNode(Token relatedToken)
            : base(relatedToken)
        {
            kind = ConstructedTypeNode.KindEnum.Interface;
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

            if (IsPartial)
            {
                sb.Append("partial ");
            }

			sb.Append("interface ");
			name.ToSource(sb);
			sb.Append(" ");

            if (IsGeneric)
            {
                Generic.TypeParametersToSource(sb);
            }

			if (baseClasses != null && baseClasses.Count > 0)
			{
				sb.Append(": ");
                baseClasses.ToSource(sb);
			}

            if (IsGeneric)
            {
                this.NewLine(sb);
                Generic.ConstraintsToSource(sb);
            }

			this.NewLine(sb);
			sb.Append("{");
			indent++;
			this.NewLine(sb);

			if (properties != null)
			{
				properties.ToSource(sb);
			}
			if (methods != null)
			{
				methods.ToSource(sb);
			}
			if (indexers != null)
			{
				indexers.ToSource(sb);
			}
			if (events != null)
			{
				events.ToSource(sb);
			}

			indent--;
			this.NewLine(sb);
			sb.Append("}");
			this.NewLine(sb);

		}

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitInterfaceDeclaration(this, data);
        }
	}
}