using System;
using System.Collections.Generic;
using System.Text;
using DDW.Collections;

namespace DDW
{
    public class ClassNode : ConstructedTypeNode
	{
        private TypeCollection baseClasses;
        public TypeCollection BaseClasses
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (baseClasses == null) baseClasses = new TypeCollection(); return baseClasses; }
		}

        private NodeCollection<EnumNode> enums = new NodeCollection<EnumNode>();
        public NodeCollection<EnumNode> Enums
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return enums; }
            [System.Diagnostics.DebuggerStepThrough]
            set { enums = value; }
        }
        
        private NodeCollection<ClassNode> classes = new NodeCollection<ClassNode>();
        public NodeCollection<ClassNode> Classes
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return classes; }
            [System.Diagnostics.DebuggerStepThrough]
            set { classes = value; }
        }

        private NodeCollection<DelegateNode> delegates = new NodeCollection<DelegateNode>();
        public NodeCollection<DelegateNode> Delegates
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return delegates; }
            [System.Diagnostics.DebuggerStepThrough]
            set { delegates = value; }
        }

        private NodeCollection<InterfaceNode> interfaces = new NodeCollection<InterfaceNode>();
        public NodeCollection<InterfaceNode> Interfaces
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return interfaces; }
            [System.Diagnostics.DebuggerStepThrough]
            set { interfaces = value; }
        }

        private NodeCollection<StructNode> structs = new NodeCollection<StructNode>();
        public NodeCollection<StructNode> Structs
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return structs; }
            [System.Diagnostics.DebuggerStepThrough]
            set { structs = value; }
        }
        
        private NodeCollection<ConstantNode> constants;
		public NodeCollection<ConstantNode> Constants
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (constants == null) constants = new NodeCollection<ConstantNode>(); return constants; }
		}

		private NodeCollection<FieldNode> fields;
		public NodeCollection<FieldNode> Fields
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (fields == null) fields = new NodeCollection<FieldNode>(); return fields; }
		}

		private NodeCollection<PropertyNode> properties;
		public NodeCollection<PropertyNode> Properties
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (properties == null) properties = new NodeCollection<PropertyNode>(); return properties; }
            [System.Diagnostics.DebuggerStepThrough]
            set { properties = value; }
		}

		private NodeCollection<ConstructorNode> constructors;
		public NodeCollection<ConstructorNode> Constructors
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (constructors == null) constructors = new NodeCollection<ConstructorNode>(); return constructors; }
            [System.Diagnostics.DebuggerStepThrough]
            set { constructors = value; }
		}

		private NodeCollection<DestructorNode> destructors;
		public NodeCollection<DestructorNode> Destructors
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (destructors == null) destructors = new NodeCollection<DestructorNode>(); return destructors; }
            [System.Diagnostics.DebuggerStepThrough]
            set { destructors = value; }
		}

		private NodeCollection<MethodNode> methods;
		public NodeCollection<MethodNode> Methods
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (methods == null) methods = new NodeCollection<MethodNode>(); return methods; }
            [System.Diagnostics.DebuggerStepThrough]
            set { methods = value; }
		}

        private NodeCollection<OperatorNode> operators;
        public NodeCollection<OperatorNode> Operators
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { if (operators == null) operators = new NodeCollection<OperatorNode>(); return operators; }
            [System.Diagnostics.DebuggerStepThrough]
            set { operators = value; }
        }

		private NodeCollection<IndexerNode> indexers;
		public NodeCollection<IndexerNode> Indexers
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (indexers == null) indexers = new NodeCollection<IndexerNode>(); return indexers; }
            [System.Diagnostics.DebuggerStepThrough]
            set { indexers = value; }
		}

		private NodeCollection<EventNode> events;
		public NodeCollection<EventNode> Events
		{
            [System.Diagnostics.DebuggerStepThrough]
            get { if (events == null) events = new NodeCollection<EventNode>(); return events; }
            [System.Diagnostics.DebuggerStepThrough]
            set { events = value; }
		}

        private NodeCollection<FixedBufferNode> fixedBuffers = new NodeCollection<FixedBufferNode>();
        public NodeCollection<FixedBufferNode> FixedBuffers
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return fixedBuffers;
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                fixedBuffers = value;
            }
        }

		//private NodeCollection<FieldNode> members;
		//public NodeCollection<FieldNode> Members
		//{
		//    get { return members; }
		//    set { members = value; }
        //}

        public ClassNode(Token relatedToken) : base(relatedToken)
        {
            kind = ConstructedTypeNode.KindEnum.Class;
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

            sb.Append(kind.ToString().ToLower() + " ");
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

            if (interfaces != null)
                interfaces.ToSource(sb);

            if (structs != null)
                structs.ToSource(sb);

            if (classes != null)
                classes.ToSource(sb);

            if (delegates != null)
                delegates.ToSource(sb);

            if (enums != null)
                enums.ToSource(sb);

            if (constants != null)
            {
                constants.ToSource(sb);
            }
            if (fields != null)
            {
                fields.ToSource(sb);
            }
            if (properties != null)
            {
                properties.ToSource(sb);
            }
            if (constructors != null)
            {
                constructors.ToSource(sb);
            }
            if (methods != null)
            {
                methods.ToSource(sb);
            }

            if (operators != null)
            {
                operators.ToSource(sb);
            }

            if (indexers != null)
            {
                indexers.ToSource(sb);
            }
            if (events != null)
            {
                events.ToSource(sb);
            }

            if (fixedBuffers != null && fixedBuffers.Count > 0)
            {
                fixedBuffers.ToSource(sb);
            }

            if (destructors != null)
            {
                destructors.ToSource(sb);
            }

            indent--;
            this.NewLine(sb);
            sb.Append("}");
            this.NewLine(sb);
        }

        protected internal override void Resolve(IResolver resolver, bool canEnterContext)
        {
            base.Resolve(resolver, false);

            if (canEnterContext)
                resolver.Context.Enter(this.name.Identifier, false);

            for (int i = 0; i < this.BaseClasses.Count; i++)
            {
                if (this.BaseClasses[i] is TypeNode &&
                    !(this.BaseClasses[i] is IdentifiedTypeNode))
                {
                    IdentifiedTypeNode node = resolver.IdentifyType((TypeNode)this.BaseClasses[i]);

                    if (node != null)
                        this.BaseClasses[i] = node;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            foreach (EnumNode node in this.Enums)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (ClassNode node in this.Classes)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (DelegateNode node in this.Delegates)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (InterfaceNode node in this.Interfaces)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (StructNode node in this.Structs)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (ConstantNode node in this.Constants)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (FieldNode node in this.Fields)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (PropertyNode node in this.Properties)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }


            foreach (ConstructorNode node in this.Constructors)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (DestructorNode node in this.Destructors)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (MethodNode node in this.Methods)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (OperatorNode node in this.Operators)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (IndexerNode node in this.Indexers)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (EventNode node in this.Events)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            foreach (FixedBufferNode node in this.FixedBuffers)
            {
                node.Parent = this;
                node.Resolve(resolver);
            }

            if (canEnterContext)
                resolver.Context.Leave();
        }

        public override object AcceptVisitor(AbstractVisitor visitor, object data)
        {
            return visitor.VisitClassDeclaration(this, data);
        }
	}
}
